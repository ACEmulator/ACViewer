float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
float3 xCamPos;
float3 xCamUp;
float xPointSpriteSizeX;
float xPointSpriteSizeY;
float xOpacity;
float xAmbient;
float3 xLightDirection;

Texture2DArray xTextures;
Texture2DArray xOverlays;
Texture2DArray xAlphas;

SamplerState TextureSampler
{
    Texture = <xTextures>;
    MinFilter = Anisotropic;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
    MaxAnisotropy = 16;
};

struct VertexPositionColor
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR;
};

struct VertexPositionNormalTexture
{
    float4 Position : SV_POSITION;
    float4 Normal : NORMAL;
    float2 TextureCoord : TEXCOORD0;
};

struct VertexPositionNormalTextures
{
    float4 Position : SV_POSITION;
    float4 Normal : NORMAL;
    float3 TextureCoord : TEXCOORD0;
};

struct VertexInstance
{
    float3 Position : POSITION1;
    float4 Orientation : TEXCOORD1;
    float3 Scale : TEXCOORD2;
};

struct VertexInstanceEnv
{
    float4 Position : POSITION1;
    float4 Rotation : TEXCOORD1;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    //float3 Normal : TEXCOORD0;
    float4 Color : COLOR0;
    float LightingFactor : COLOR1;
    float3 TextureCoord : TEXCOORD0;
    float2 AlphaCoord : TEXCOORD1;
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};

//------- Technique: ColoredNoShading --------

VertexShaderOutput ColoredNoShadingVS(VertexPositionColor vertex)
{
    VertexShaderOutput Output = (VertexShaderOutput)0;

    float4x4 preViewProjection = mul(xView, xProjection);
    float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);

    Output.Position = mul(vertex.Position, preWorldViewProjection);
    Output.Color = vertex.Color;

    return Output;
}

float4 ColoredNoShadingPS(VertexShaderOutput vsData) : COLOR
{
    return vsData.Color;
}

technique ColoredNoShading
{
    pass Pass0
    {
        VertexShader = compile vs_4_0 ColoredNoShadingVS();
        PixelShader = compile ps_4_0 ColoredNoShadingPS();
    }
}

//------- Technique: TexturedInstance --------

matrix<float, 4, 4> QuaternionToMatrix(float4 q)
{
    float xx = q.x * q.x;
    float yy = q.y * q.y;
    float zz = q.z * q.z;
    float xy = q.x * q.y;
    float zw = q.z * q.w;
    float zx = q.z * q.x;
    float yw = q.y * q.w;
    float yz = q.y * q.z;
    float xw = q.x * q.w;

    matrix<float, 4, 4> result =
    {
        1 - (2 * (yy + zz)),
        2 * (xy + zw),
        2 * (zx - yw),
        0,
        2 * (xy - zw),
        1 - (2 * (zz + xx)),
        2 * (yz + xw),
        0,
        2 * (zx + yw),
        2 * (yz - xw),
        1 - (2 * (yy + xx)),
        0, 0, 0, 0, 1
    };

    return result;
}

VertexShaderOutput TexturedInstanceVS(VertexPositionNormalTextures model, VertexInstance instance)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    // pre-compute matrix?
    matrix<float, 4, 4> translation =
    {
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        instance.Position.x, instance.Position.y, instance.Position.z, 1
    };

    matrix<float, 4, 4> rotation = QuaternionToMatrix(instance.Orientation);

    matrix<float, 4, 4> scale =
    {
        instance.Scale.x, 0, 0, 0,
        0, instance.Scale.y, 0, 0,
        0, 0, instance.Scale.z, 0,
        0, 0, 0, 1
    };

    float4 instancePos = mul(mul(mul(model.Position, rotation), scale), translation);

    output.Position = mul(mul(instancePos, xView), xProjection);

    //output.Normal = input.Normal;
    output.TextureCoord = model.TextureCoord;

    output.LightingFactor = saturate(dot(model.Normal.xyz, -xLightDirection)) + xAmbient;

    return output;
}

float4 TexturedInstancePS(VertexShaderOutput input) : COLOR
{
    float4 color = xTextures.Sample(TextureSampler, input.TextureCoord);

    // only output completely opaque pixels
    clip(color.a < 1.0f ? -1 : 1);

    color.rgb *= input.LightingFactor;

    return color;
}

float4 TexturedInstanceTransPS(VertexShaderOutput input) : COLOR
{
    float4 color = xTextures.Sample(TextureSampler, input.TextureCoord);

    // only output semi-transparent pixels
    clip(color.a < 1.0f ? 1 : -1);

    color.rgb *= input.LightingFactor;

    return color;
}

technique TexturedInstance
{
    pass Pass0
    {
        ZWriteEnable = true;

        VertexShader = compile vs_4_0 TexturedInstanceVS();
        PixelShader = compile ps_4_0 TexturedInstancePS();
    }
}

technique TexturedInstanceAlpha
{
    pass Pass0
    {
        ZWriteEnable = true;

        VertexShader = compile vs_4_0 TexturedInstanceVS();
        PixelShader = compile ps_4_0 TexturedInstancePS();
    }
    pass Pass1
    {
        ZWriteEnable = false;

        AlphaBlendEnable = true;
        DestBlend = InvSrcAlpha;
        SrcBlend = SrcAlpha;

        VertexShader = compile vs_4_0 TexturedInstanceVS();
        PixelShader = compile ps_4_0 TexturedInstanceTransPS();
    }
}

//------- Technique: TexturedInstanceEnv --------

VertexShaderOutput TexturedInstanceEnvVS(VertexPositionNormalTextures model, VertexInstanceEnv instance)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    matrix<float, 4, 4> translation =
    {
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        instance.Position.x, instance.Position.y, instance.Position.z, 1
    };

    matrix<float, 4, 4> rotation = QuaternionToMatrix(instance.Rotation);

    // model position = identity?
    // instance position (translation) = blockPos + posInBlock
    // instance.Rotation = frame.Rotation

    float4 instancePos = mul(mul(model.Position, rotation), translation);

    float4x4 preViewProjection = mul(xView, xProjection);
    float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);

    output.Position = mul(instancePos, preWorldViewProjection);
    //output.Normal = input.Normal;
    output.TextureCoord = model.TextureCoord;

    output.LightingFactor = saturate(dot(model.Normal.xyz, -xLightDirection)) + xAmbient;

    return output;
}

technique TexturedInstanceEnv
{
    pass Pass0
    {
        ZWriteEnable = true;

        VertexShader = compile vs_4_0 TexturedInstanceEnvVS();
        PixelShader = compile ps_4_0 TexturedInstancePS();
    }
    /*pass Pass1
    {
        ZWriteEnable = false;

        AlphaBlendEnable = true;
        DestBlend = InvSrcAlpha;
        SrcBlend = SrcAlpha;

        VertexShader = compile vs_4_0 TexturedInstanceEnvVS();
        PixelShader = compile ps_4_0 TexturedInstanceTransPS();
    }*/
}

//------- Technique: TexturedNoShading --------

VertexShaderOutput TexturedNoShadingVS(float4 iPos : SV_POSITION, float2 iTexCoord : TEXCOORD0)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPos = mul(iPos, xWorld);
    float4 viewPos = mul(worldPos, xView);
    output.Position = mul(viewPos, xProjection);

    //output.Normal = input.Normal;
    output.TextureCoord = float3(iTexCoord, 0);

    return output;
}

PixelShaderOutput TexturedNoShadingPS(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput)0;

    output.Color = xTextures.Sample(TextureSampler, float3(input.TextureCoord.xy, 0));

    output.Color.a *= xOpacity;

    // only output completely opaque pixels
    clip(output.Color.a < 1.0f ? -1 : 1);

    return output;
}

PixelShaderOutput TexturedNoShadingTransPS(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput)0;

    output.Color = xTextures.Sample(TextureSampler, float3(input.TextureCoord.xy, 0));

    output.Color.a *= xOpacity;

    // only output semi-transparent pixels
    clip(output.Color.a < 1.0f ? 1 : -1);

    return output;
}

technique TexturedNoShading
{
    pass Pass0
    {
        ZWriteEnable = true;

        VertexShader = compile vs_4_0 TexturedNoShadingVS();
        PixelShader = compile ps_4_0 TexturedNoShadingPS();
    }
    pass Pass1
    {
        ZWriteEnable = false;

        AlphaBlendEnable = true;
        DestBlend = InvSrcAlpha;
        SrcBlend = SrcAlpha;

        VertexShader = compile vs_4_0 TexturedNoShadingVS();
        PixelShader = compile ps_4_0 TexturedNoShadingTransPS();
    }
}

//------- Technique: TexturedNoShadingAlphaOnly --------

VertexShaderOutput TexturedNoShadingAlphaOnlyVS(float4 iPos : SV_POSITION, float2 iTexCoord : TEXCOORD0)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPos = mul(iPos, xWorld);
    float4 viewPos = mul(worldPos, xView);
    output.Position = mul(viewPos, xProjection);

    //output.Normal = input.Normal;
    output.TextureCoord = float3(iTexCoord, 0);

    return output;
}

PixelShaderOutput TexturedNoShadingAlphaOnlyPS(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput)0;

    output.Color = xTextures.Sample(TextureSampler, float3(input.TextureCoord.xy, 0));

    // only output completely opaque pixels
    output.Color.a *= xOpacity;

    return output;
}

technique TexturedNoShadingAlphaOnly
{
    pass Pass0
    {
        ZWriteEnable = false;

        VertexShader = compile vs_4_0 TexturedNoShadingAlphaOnlyVS();
        PixelShader = compile ps_4_0 TexturedNoShadingAlphaOnlyPS();
    }
}

//------- Technique: PointSprite --------

VertexShaderOutput PointSpriteVS(float4 iPos : SV_POSITION, float2 iTexCoord : TEXCOORD0)
{
    VertexShaderOutput Output = (VertexShaderOutput)0;

    float3 center = mul(iPos, xWorld).xyz;
    float3 eyeVector = center - xCamPos;

    float3 sideVector = cross(eyeVector, xCamUp);
    sideVector = normalize(sideVector);
    float3 upVector = cross(sideVector, eyeVector);
    upVector = normalize(upVector);

    float3 finalPosition = center;
    finalPosition += (iTexCoord.x - 0.5f) * sideVector * 0.5f * xPointSpriteSizeX;
    finalPosition += (0.5f - iTexCoord.y) * upVector * 0.5f * xPointSpriteSizeY;

    float4 finalPosition4 = float4(finalPosition, 1);

    float4x4 preViewProjection = mul(xView, xProjection);
    Output.Position = mul(finalPosition4, preViewProjection);

    Output.TextureCoord = float3(iTexCoord, 0);

    return Output;
}

PixelShaderOutput PointSpritePS(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput)0;
    output.Color = xTextures.Sample(TextureSampler, float3(input.TextureCoord.xy, 0));

    // only output completely opaque pixels
    output.Color.a *= xOpacity;
    clip(output.Color.a < 1.0f ? -1 : 1);

    return output;
}

PixelShaderOutput PointSpriteTransPS(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput)0;
    output.Color = xTextures.Sample(TextureSampler, float3(input.TextureCoord.xy, 0));

    // only output semi-transparent pixels
    output.Color.a *= xOpacity;

    clip(output.Color.a < 1.0f ? 1 : -1);

    return output;
}

technique PointSprite
{
    pass Pass0
    {
        VertexShader = compile vs_4_0 PointSpriteVS();
        PixelShader = compile ps_4_0 PointSpritePS();
    }
    pass Pass1
    {
        ZWriteEnable = false;

        VertexShader = compile vs_4_0 PointSpriteVS();
        PixelShader = compile ps_4_0 PointSpriteTransPS();
    }
}

struct BaseData
{
    float4 Position : SV_POSITION;
    float3 TexUV : TEXCOORD0;
};

struct TerrainData
{
    float4 Overlay0 : TEXCOORD1;
    float4 Overlay1 : TEXCOORD2;
    float4 Overlay2 : TEXCOORD3;
};

struct RoadData
{
    float4 Road0 : TEXCOORD4;
    float4 Road1 : TEXCOORD5;
};

struct VertexShaderInput
{
    float4 Position : SV_POSITION;
    float4 Normal : NORMAL0;
    float3 TextureCoord : TEXCOORD0;
    float4 Overlay0 : TEXCOORD1;
    float4 Overlay1 : TEXCOORD2;
    float4 Overlay2 : TEXCOORD3;
    float4 Road0 : TEXCOORD4;
    float4 Road1 : TEXCOORD5;
};

struct PSCombined
{
    BaseData Base;
    TerrainData Terrains;
    RoadData Roads;
    float LightingFactor : COLOR0;
};

//------- Technique: LandscapeSinglePass --------

// thanks to parad0x for authoring these shaders!

float4 maskBlend3(float4 t0, float4 t1, float4 t2, float h0, float h1, float h2)
{
    float1 a0 = h0 == 0 ? 1 : t0.a;
    float1 a1 = h1 == 0 ? 1 : t1.a;
    float1 a2 = h2 == 0 ? 1 : t2.a;
    float1 aR = 1 - (a0 * a1 * a2);

    a0 = 1 - a0;
    a1 = 1 - a1;
    a2 = 1 - a2;

    float3 r0 = (a0 * t0.rgb + (1 - a0) * a1 * t1.rgb + (1 - a1) * a2 * t2.rgb);

    float4 r;
    r.a = aR;
    r.rgb = (1 / aR) * r0;

    return r;
}

float4 CombineOverlays(BaseData base, TerrainData terrains)
{
    // these will zero-out any unused textures
    // dx unfortunately defaults to black, full alpha
    // ogl defaults to black, zero alpha

    float h0 = terrains.Overlay0.z < 0 ? 0 : 1;
    float h1 = terrains.Overlay1.z < 0 ? 0 : 1;
    float h2 = terrains.Overlay2.z < 0 ? 0 : 1;

    float4 overlay0 = 0;
    float4 overlay1 = 0;
    float4 overlay2 = 0;
    float4 overlayAlpha0 = 0;
    float4 overlayAlpha1 = 0;
    float4 overlayAlpha2 = 0;

    float2 uvb = base.TexUV.xy;

    float4 result;

    if (h0 > 0)
    {
        overlay0 = xOverlays.Sample(TextureSampler, float3(uvb, terrains.Overlay0.z));
        overlayAlpha0 = xAlphas.Sample(TextureSampler, float3(terrains.Overlay0.xy, terrains.Overlay0.w));
        overlay0.a = overlayAlpha0.a;
    }

    if (h1 > 0)
    {
        overlay1 = xOverlays.Sample(TextureSampler, float3(uvb, terrains.Overlay1.z));
        overlayAlpha1 = xAlphas.Sample(TextureSampler, float3(terrains.Overlay1.xy, terrains.Overlay1.w));
        overlay1.a = overlayAlpha1.a;
    }

    if (h2 > 0)
    {
        overlay2 = xOverlays.Sample(TextureSampler, float3(uvb, terrains.Overlay2.z));
        overlayAlpha2 = xAlphas.Sample(TextureSampler, float3(terrains.Overlay2.xy, terrains.Overlay2.w));
        overlay2.a = overlayAlpha2.a;
    }

    result = maskBlend3(overlay0, overlay1, overlay2, h0, h1, h2);

    return result;
}

float4 CombineRoad(BaseData base, RoadData roads)
{
    float h0 = roads.Road0.z < 0 ? 0 : 1;
    float h1 = roads.Road1.z < 0 ? 0 : 1;

    float2 uvb = base.TexUV.xy;

    float4 result = 0;

    if (h0 > 0)
    {
        result = xOverlays.Sample(TextureSampler, float3(uvb, roads.Road0.z));
        float4 roadAlpha0 = xAlphas.Sample(TextureSampler, float3(roads.Road0.xy, roads.Road0.w));
        result.a = 1 - roadAlpha0.a;

        if (h1 > 0)
        {
            float4 roadAlpha1 = xAlphas.Sample(TextureSampler, float3(roads.Road1.xy, roads.Road1.w));
            result.a = 1 - (roadAlpha0.a * roadAlpha1.a);
        }
    }
    return result;
}

PSCombined LandscapeSinglePassVS(VertexShaderInput input)
{
    PSCombined output = (PSCombined)0;

    BaseData base = (BaseData)0;

    base.Position = mul(mul(mul(input.Position, xWorld), xView), xProjection);
    base.TexUV = input.TextureCoord;

    output.Base = base;

    TerrainData terrains = (TerrainData)0;

    terrains.Overlay0 = input.Overlay0;
    terrains.Overlay1 = input.Overlay1;
    terrains.Overlay2 = input.Overlay2;

    output.Terrains = terrains;

    RoadData roads = (RoadData)0;

    roads.Road0 = input.Road0;
    roads.Road1 = input.Road1;

    output.Roads = roads;

    float3 normal = normalize(mul(input.Normal, xWorld)).xyz;

    output.LightingFactor = dot(normal, -xLightDirection);

    return output;
}

float4 LandscapeSinglePassPS(PSCombined input) : COLOR
{
    float4 baseColor = xOverlays.Sample(TextureSampler, input.Base.TexUV.xyz);

    float4 combinedOverlays = 0;
    float4 combinedRoad = 0;

    if (input.Terrains.Overlay0.z >= 0)
        combinedOverlays = CombineOverlays(input.Base, input.Terrains);

    if (input.Roads.Road0.z >= 0)
        combinedRoad = CombineRoad(input.Base, input.Roads);

    float3 baseMasked = saturate(baseColor.rgb * ((1 - combinedOverlays.a) * (1 - combinedRoad.a)));
    float3 overlaysMasked = saturate(combinedOverlays.rgb * (combinedOverlays.a * (1 - combinedRoad.a)));
    float3 roadMasked = combinedRoad.rgb * combinedRoad.a;

    float4 color = float4(baseMasked + overlaysMasked + roadMasked, 1);

    color.rgb *= saturate(input.LightingFactor) + xAmbient;

    return color;
}

technique LandscapeSinglePass
{
    pass Pass0
    {
        ZWriteEnable = true;

        VertexShader = compile vs_4_0 LandscapeSinglePassVS();
        PixelShader = compile ps_4_0 LandscapeSinglePassPS();
    }
}

//------- Technique: ParticleInstance --------

struct VertexParticleBase
{
    float4 Position : SV_POSITION;
    float2 TextureCoord : TEXCOORD0;
};

struct VertexParticleInstance
{
    float4 Position : POSITION1;
    float3 BillboardTexture : TEXCOORD1;
    float3 ScaleOpacityActive : POSITION2;
};

struct ParticleVertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 TextureCoordIdx : TEXCOORD0;
    float2 OpacityActive : POSITION1;
};

ParticleVertexShaderOutput ParticleInstanceVS(VertexParticleBase base, VertexParticleInstance instance)
{
    ParticleVertexShaderOutput output = (ParticleVertexShaderOutput)0;

    // xWorld?
    float3 iPos = (base.Position + instance.Position).xyz;

    float3 center = iPos;
    float3 eyeVector = center - xCamPos;

    float3 sideVector = cross(eyeVector, xCamUp);
    sideVector = normalize(sideVector);
    float3 upVector = cross(sideVector, eyeVector);
    upVector = normalize(upVector);

    float3 finalPosition = center;
    finalPosition += (base.TextureCoord.x - 0.5f) * sideVector * 0.5f * instance.BillboardTexture.x * instance.ScaleOpacityActive.x;
    finalPosition += (0.5f - base.TextureCoord.y) * upVector * 0.5f * instance.BillboardTexture.y * instance.ScaleOpacityActive.x;

    float4 finalPosition4 = float4(finalPosition, 1);

    float4x4 preViewProjection = mul(xView, xProjection);

    output.Position = mul(finalPosition4, preViewProjection);

    // pixel shader needs:
    // transformed position
    // base texture u/v
    // texture idx
    // opacity
    // active
    output.TextureCoordIdx = float3(base.TextureCoord.xy, instance.BillboardTexture.z);

    output.OpacityActive = float2(instance.ScaleOpacityActive.yz);

    return output;
}

float4 ParticleInstancePS(ParticleVertexShaderOutput input) : COLOR
{
    float4 color = xTextures.Sample(TextureSampler, input.TextureCoordIdx);

    color.a *= input.OpacityActive.x;

    // discard inactive particles
    clip(input.OpacityActive.y == 0 ? -1 : 1);

    return color;
}

technique ParticleInstance
{
    pass Pass0
    {
        ZWriteEnable = false;

        VertexShader = compile vs_4_0 ParticleInstanceVS();
        PixelShader = compile ps_4_0 ParticleInstancePS();
    }
}

//------- Technique: Picker --------

VertexPositionColor PickerVS(float4 inPos : SV_POSITION, float4 inColor : COLOR)
{
    VertexPositionColor output = (VertexPositionColor)0;

    output.Position = mul(mul(inPos, xView), xProjection);
    output.Color = inColor;

    return output;
}

PixelShaderOutput PickerPS(VertexPositionColor input)
{
    PixelShaderOutput output = (PixelShaderOutput)0;

    output.Color = input.Color;

    return output;
}

technique Picker
{
    pass Pass0
    {
        ZEnable = false;

        VertexShader = compile vs_4_0 ColoredNoShadingVS();
        PixelShader = compile ps_4_0 ColoredNoShadingPS();
    }
}
