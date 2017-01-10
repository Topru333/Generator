#pragma once
#include <iostream>
// Вектор в пространстве R^3
struct Vec3
{
	float x, y, z;
public:
	// Возвращает случайный вектор 
	static Vec3 GetRandomVector() {
		return{ (rand() % 100 + 1) / 100.0f,(rand() % 100 + 1) / 100.0f,(rand() % 100 + 1) / 100.0f };
	}
	static Vec3 GetRandomUnitVector() {
		return GetRandomVector().Normalized();
	}
	float Length() {
		return sqrtf(x*x + y*y + z*z);
	}
	float Dot(Vec3 a) {
		return a.x*x + a.y*y + a.z*z;
	}
	Vec3 Diff(Vec3 a) {
		float length = Length();
		return{ x - a.x, y - a.y, z - a.z };
	}
	Vec3 Normalized() {
		float length = Length();
		return{ x / length,y / length,z / length };
	}
	static Vec3 Up() {
		return{ 0,1,0 };
	}
	static Vec3 Down() {
		return{ 0,-1,0 };
	}
	static Vec3 Right() {
		return{ 1,0,0 };
	}
	static Vec3 Left() {
		return{ -1,0,0 };
	}
	static Vec3 Fwd() {
		return{ 0,0,1 };
	}
	static Vec3 Bck() {
		return{ 0,0,-1 };
	}
	Vec3 ProjectOnSphere(Vec3 pos)
	{
		return pos.Normalized();
	}
};