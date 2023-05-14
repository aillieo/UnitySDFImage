#ifndef SDF_2D_INCLUDED
#define SDF_2D_INCLUDED

#define PI 3.141592653589793238

float smin(float a, float b, float k) {
    float h = max(k - abs(a - b), 0.0f) / k;
    return min(a, b) - h * h * k * 0.25f;
}

float smax(float a, float b, float k) {
    float h = max(k - abs(a - b), 0.0f) / k;
    return max(a, b) + h * h * k * 0.25f;
}

float distanceToLine(float2 pt, float2 p0, float2 p1)
{
    float2 v = p1 - p0;
    float2 w = pt - p0;    
    float c1 = dot(w, v);
    float c2 = dot(v, v);    
    return length(w - v * (c1 / c2));
}

#endif
