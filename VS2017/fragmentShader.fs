#version 330 core

out vec4 FragColor;

#define LINE_WIDTH 2.5
uniform vec3 lightPos;
uniform vec3 viewPos;
uniform vec4 lightColor;
uniform vec4 objectColor;

uniform float far_plane;

uniform sampler2D textureSampler;
uniform samplerCube depthMap;

uniform bool stateOfTexture;
uniform bool shadows;


in vec2 vertexUV;
in vec3 Normal;
in vec3 FragPos;
    
float ShadowCalculation(vec3 fragPos)
{
    // get vector between fragment position and light position
    vec3 fragToLight = fragPos - lightPos;
    // ise the fragment to light vector to sample from the depth map    
    float closestDepth = texture(depthMap, fragToLight).r;
    // it is currently in linear range between [0,1], let's re-transform it back to original depth value
    closestDepth *= far_plane;
    // now get current linear depth as the length between the fragment and light position
    float currentDepth = length(fragToLight);
    // test for shadows
    float bias = 0.05; // we use a much larger bias since depth is now in [near_plane, far_plane] range
    float shadow = currentDepth -  bias > closestDepth ? 1.0 : 0.0;        
    // display closestDepth as debug (to visualize depth cubemap)
    // FragColor = vec4(vec3(closestDepth / far_plane), 1.0);    
        
    return shadow;
}


void main()
{
    float ambientStrength = 0.3;
    vec3 ambient = ambientStrength * vec3(lightColor);
        
    vec3 norm = normalize(Normal);

    vec3 lightDir = normalize(lightPos - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * vec3(lightColor);
        
    float specularStrength = 0.9;
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    vec3 specular = specularStrength * spec * vec3(lightColor);
        
    vec4 textureColor = texture( textureSampler, vertexUV );

    float shadow = shadows ? ShadowCalculation(FragPos) : 0.0;

    vec3 result = (ambient + (1.0 - shadow)*(diffuse + specular)) ;
    
    if (stateOfTexture)FragColor = textureColor * vec4(result, 1.0); 
    else FragColor =  vec4(result * vec3(objectColor), 1.0);
}