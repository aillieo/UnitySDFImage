#ifndef SDF_2D_INCLUDED
#define SDF_2D_INCLUDED

float smin(float a, float b, float k) {
    float h = max(k - abs(a - b), 0.0f) / k;
    return min(a, b) - h * h * k * 0.25f;
}

float smax(float a, float b, float k) {
    float h = max(k - abs(a - b), 0.0f) / k;
    return max(a, b) + h * h * k * 0.25f;
}

#endif
