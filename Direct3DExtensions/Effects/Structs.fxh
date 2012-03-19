#ifndef STRUCTS_H
#define STRUCTS_H

cbuffer PerFrame
{
	float4x4 World;
	float4x4 View;
	float4x4 Proj;
	float4x4 InvProj;
	float3 CameraPos;
};

struct VS_POS
{
	float3 pos : POSITION;
};

struct VS_POS_TEX
{
	float3 pos : POSITION;
	float3 uv  : TEXCOORD;
};

struct VS_POS_NORM
{
	float3 pos : POSITION;
	float3 norm : NORMAL;
};

struct VS_POS_NORM_TEX
{
	float3 pos : POSITION;
	float3 norm : NORMAL;
	float2 uv : TEXCOORD;
};

struct PS_POS
{
	float4 pos : SV_POSITION;
};

struct PS_TEX
{
	float4 pos   : SV_POSITION;
	float3 uv    : TEXCOORD;
};

struct PS_NORM
{
	float4 pos : SV_POSITION;
	float3 norm : NORMAL;
};

struct PS_NORM_TEX
{
	float4 pos : SV_POSITION;
	float3 norm : NORMAL;
	float3 uv : TEXCOORD;
};

#endif