#define DllExport __declspec (dllexport)
#include <math.h>
#include "noise.h"
#include "vec3.h"


float Lerp(float a, float b, float t) {
	return a*(1.0f - t) + b*t;
}
float perlinFade(float t) {
	return t * t * t * (t * (t * 6 - 15) + 10);
}
int *p;
double fade(double t) { return t * t * t * (t * (t * 6 - 15) + 10); }
double lerp(double t, double a, double b) { return a + t * (b - a); }

double grad(int hash, double x, double y, double z) {
	int h = hash & 15;                      // CONVERT LO 4 BITS OF HASH CODE
	double u = h<8 ? x : y,                 // INTO 12 GRADIENT DIRECTIONS.
		v = h<4 ? y : h == 12 || h == 14 ? x : z;
	return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
}

double pnoise(double x, double y, double z) {
	// ���������� ������ � ������� ��������� ���
	int X = (int)floor(x) & 255,                  
		Y = (int)floor(y) & 255,
		Z = (int)floor(z) & 255;
	// ����� ������������ ��������� ���������
	x -= floor(x);                               
	y -= floor(y);                               
	z -= floor(z);
	// ������������ �������� ������������
	double	u = fade(x),                                
			v = fade(y),                                
			w = fade(z);
	// �������� ������� �����
	int A = p[X] + Y,	  AA = p[A] + Z, AB = p[A + 1] + Z,      
		B = p[X + 1] + Y, BA = p[B] + Z, BB = p[B + 1] + Z;      
	// ���������� �������� ���� � ����� xyz
	return lerp(w,	lerp(v,	lerp(u,	grad(p[AA], x, y, z),
									grad(p[BA], x - 1, y, z)),
							lerp(u, grad(p[AB], x, y - 1, z),  
									grad(p[BB], x - 1, y - 1, z))),
					lerp(v, lerp(u, grad(p[AA + 1], x, y, z - 1), 
									grad(p[BA + 1], x - 1, y, z - 1)), 
							lerp(u, grad(p[AB + 1], x, y - 1, z - 1),
									grad(p[BB + 1], x - 1, y - 1, z - 1))));
}

bool ComputeTerrainTexture(float* noiseAtPoints, Vec3*	verticles, size_t vCount, size_t Size, float scale) {
	for (size_t i = 0; i < vCount; i++)
	{
		//TODO: ����������� ��� ���������� ����
	}
	return true;
}

void InitPerm() {
	// �������� 512 ��������� ����� ����� �� 0 �� 255
	p = (int*)malloc(512*sizeof(int));
	for (int i = 0; i < 256; i++) 
		p[256 + i] = p[i] = rand()%256;
}
void DeinitPerm() {
	free(p);
}
extern "C" {

	// �������� ������� ���������� ��������� �������� TODO:������
	DllExport int RandomTest() {
		return rand();
	}
	// ���������� ������ ����������� ����� �������.
	DllExport bool PerlinNoise(	Vec3*	cubeFaces[6],		// ������ �� ������� ������ 6 ������ ����
								int		vCount[6],
								int		Size,				// ������ ������������ �������
								float*  noiseTextures[6],	// ������ �� ������� 6 ������ ����
								float	noiseScale			// ���������� ����
	) 
	{
		InitPerm();
		for (size_t cubeFace = 0; cubeFace < 6; cubeFace++)
			ComputeTerrainTexture(noiseTextures[cubeFace],cubeFaces[cubeFace], vCount[cubeFace], Size, noiseScale);
		DeinitPerm();
		return true;
	}
	DllExport void InitLib()
	{
		InitPerm();
	}

	DllExport void DeInitLib()
	{
		DeinitPerm();
	}
	DllExport float PerlinNoiseAtPoint(float x, float y, float z)
	{
		return (float)pnoise((float)x, (float)y, (float)z);
	}
}