/*{
    "CATEGORIES": [
        "Ray March"
    ],
    "DESCRIPTION": "",
    "IMPORTED": {
    },
    "INPUTS": [
        {
            "LABEL": "Ball Radius 1",
            "NAME": "ballRadius1",
            "TYPE": "float"
        },
        {
            "LABEL": "Ball Radius 2",
            "NAME": "ballRadius2",
            "TYPE": "float"
        },
        {
            "LABEL": "Ball Radius 3",
            "NAME": "ballRadius3",
            "TYPE": "float"
        },
        {
            "LABEL": "Ball Radius 4",
            "NAME": "ballRadius4",
            "TYPE": "float"
        },
        {
            "LABEL": "Ball Radius 5",
            "NAME": "ballRadius5",
            "TYPE": "float"
        },
        {
            "LABEL": "Ball Radius 6",
            "NAME": "ballRadius6",
            "TYPE": "float"
        },
        {
            "DEFAULT": 3,
            "LABEL": "Ground Plane Pos",
            "MAX": 6.28,
            "MIN": 0,
            "NAME": "groundPlanePos",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0,
            "LABEL": "Ball Offset",
            "MAX": 10,
            "MIN": 0,
            "NAME": "ballOffset",
            "TYPE": "float"
        },
        {
            "DEFAULT": 120,
            "LABEL": "Beats per Minute",
            "MAX": 1,
            "MIN": 240,
            "NAME": "bpm",
            "TYPE": "float"
        },
        {
            "DEFAULT": [
                1,
                1,
                1,
                1
            ],
            "LABEL": "Ball Color",
            "NAME": "ballColor",
            "TYPE": "color"
        },
        {
            "DEFAULT": [
                0.9411764705882353,
                0.9607843137254902,
                0.8156862745098039,
                1
            ],
            "LABEL": "Ground Color",
            "NAME": "groundColor",
            "TYPE": "color"
        }
    ],
    "ISFVSN": "2"
}
*/


#define PI 3.1415925359


const int MAX_MARCHING_STEPS = 100;
const float MIN_DIST = 0.0;
const float MAX_DIST = 100.0;
const float EPSILON = 0.0001;



/**
 * Rotation matrix around the X axis.
 */
mat3 rotateX(float theta) {
    float c = cos(theta);
    float s = sin(theta);
    return mat3(
        vec3(1, 0, 0),
        vec3(0, c, -s),
        vec3(0, s, c)
    );
}

/**
 * Rotation matrix around the Y axis.
 */
mat3 rotateY(float theta) {
    float c = cos(theta);
    float s = sin(theta);
    return mat3(
        vec3(c, 0, s),
        vec3(0, 1, 0),
        vec3(-s, 0, c)
    );
}

/**
 * Rotation matrix around the Z axis.
 */
mat3 rotateZ(float theta) {
    float c = cos(theta);
    float s = sin(theta);
    return mat3(
        vec3(c, -s, 0),
        vec3(s, c, 0),
        vec3(0, 0, 1)
    );
}


vec4 intersectSDF(vec4 a, vec4 b) {
    return a.w > b.w ? a : b;
}
  
vec4 unionSDF(vec4 a, vec4 b) {
    return a.w < b.w? a : b;
}
 
vec4 differenceSDF(vec4 a, vec4 b) {
    return a.w > -b.w? a : vec4(b.rgb,-b.w);
}

/**
 * Constructive solid geometry intersection operation on SDF-calculated distances.
 */
float intersectSDF(float distA, float distB) {
    return max(distA, distB);
}

/**
 * Constructive solid geometry union operation on SDF-calculated distances.
 */
float unionSDF(float distA, float distB) {
    return min(distA, distB);
}

/**
 * Constructive solid geometry difference operation on SDF-calculated distances.
 */
float differenceSDF(float distA, float distB) {
    return max(distA, -distB);
}

float planeSDF(vec3 p,vec4 n) {
    // n must be normalized
    return dot(p,n.xyz)+n.w;
}

/**
 * Signed distance function for a sphere centered at the origin with radius r.
 */
float sphereSDF(vec3 p, float r) {
    return length(p) - r;
}

/**
 * Signed distance function describing the scene.
 * 
 * Absolute value of the return value indicates the distance to the surface.
 * Sign indicates whether the point is inside or outside the surface,
 * negative indicating inside.
 */
vec4 sceneSDF(vec3 samplePoint) {    

    vec4 plane = vec4(groundColor.rgb, planeSDF(samplePoint, vec4(1,1,1,groundPlanePos)));

    float bbpm = 4.;  // beats per measure
    float spm = bbpm*60./bpm; // seconds per measure
    mat3 pointRotation = rotateY(spm * TIME) * rotateX(spm * TIME);
    samplePoint = pointRotation * samplePoint;

    float ball1 = sphereSDF(samplePoint - vec3(ballOffset, 0.0, 0.0), ballRadius1);
    vec4 balls = vec4(ballColor.rgb, ball1);
    balls = unionSDF(balls, vec4(ballColor.rgb, sphereSDF(samplePoint + vec3(ballOffset, 0.0, 0.0), ballRadius2)));
    balls = unionSDF(balls, vec4(ballColor.rgb, sphereSDF(samplePoint - vec3(0.0, ballOffset, 0.0), ballRadius3)));
    balls = unionSDF(balls, vec4(ballColor.rgb, sphereSDF(samplePoint + vec3(0.0, ballOffset, 0.0), ballRadius4)));
    balls = unionSDF(balls, vec4(ballColor.rgb, sphereSDF(samplePoint - vec3(0.0, 0.0, ballOffset), ballRadius5)));
    balls = unionSDF(balls, vec4(ballColor.rgb, sphereSDF(samplePoint + vec3(0.0, 0.0, ballOffset), ballRadius6)));
    
    return unionSDF(plane, balls);
//    return balls;
}

/**
 * Return the shortest distance from the eyepoint to the scene surface along
 * the marching direction. If no part of the surface is found between start and end,
 * return end.
 * 
 * eye: the eye point, acting as the origin of the ray
 * marchingDirection: the normalized direction to march in
 * start: the starting distance away from the eye
 * end: the max distance away from the ey to march before giving up
 */
float shortestDistanceToSurface(vec3 eye, vec3 marchingDirection, float start, float end) {
    float depth = start;
    for (int i = 0; i < MAX_MARCHING_STEPS; i++) {
        vec4 dist = sceneSDF(eye + depth * marchingDirection);
        if (dist.w < EPSILON) {
			return depth;
        }
        depth += dist.w;
        if (depth >= end) {
            return end;
        }
    }
    return end;
}
            

/**
 * Return the normalized direction to march in from the eye point for a single pixel.
 * 
 * fieldOfView: vertical field of view in degrees
 * size: resolution of the output image
 * fragCoord: the x,y coordinate of the pixel in the output image
 */
vec3 rayDirection(float fieldOfView, vec2 size, vec2 fragCoord) {
    vec2 xy = fragCoord - size / 2.0;
    float z = size.y / tan(radians(fieldOfView) / 2.0);
    return normalize(vec3(xy, -z));
}

/**
 * Using the gradient of the SDF, estimate the normal on the surface at point p.
 */
vec3 estimateNormal(vec3 p) {

    float d=sceneSDF(p).w;// Distance
    vec2 e=vec2(.01,0);// Epsilon
     
    vec3 n=d-vec3(
        sceneSDF(p-e.xyy).w,
        sceneSDF(p-e.yxy).w,
        sceneSDF(p-e.yyx).w);
         
    return normalize(n);
}


/**
 * Lighting contribution of a single point light source via Phong illumination.
 * 
 * The vec3 returned is the RGB color of the light's contribution.
 *
 * k_a: Ambient color
 * k_d: Diffuse color
 * k_s: Specular color
 * alpha: Shininess coefficient
 * p: position of point being lit
 * eye: the position of the camera
 * lightPos: the position of the light
 * lightIntensity: color/intensity of the light
 *
 * See https://en.wikipedia.org/wiki/Phong_reflection_model#Description
 */
vec3 phongContribForLight(vec3 k_d, vec3 k_s, float alpha, vec3 p, vec3 eye,
                          vec3 lightPos, vec3 lightIntensity) {
    vec3 N = estimateNormal(p);
    vec3 L = normalize(lightPos - p);
    vec3 V = normalize(eye - p);
    vec3 R = normalize(reflect(-L, N));
    
    float dotLN = dot(L, N);
    float dotRV = dot(R, V);
    
    if (dotLN < 0.0) {
        // Light not visible from this point on the surface
        return vec3(0.0, 0.0, 0.0);
    } 
    
    if (dotRV < 0.0) {
        // Light reflection in opposite direction as viewer, apply only diffuse
        // component
        return lightIntensity * (k_d * dotLN);
    }
    return lightIntensity * (k_d * dotLN + k_s * pow(dotRV, alpha));
}

/**
 * Lighting via Phong illumination.
 * 
 * The vec3 returned is the RGB color of that point after lighting is applied.
 * k_a: Ambient color
 * k_d: Diffuse color
 * k_s: Specular color
 * alpha: Shininess coefficient
 * p: position of point being lit
 * eye: the position of the camera
 *
 * See https://en.wikipedia.org/wiki/Phong_reflection_model#Description
 */
vec3 phongIllumination(vec3 k_a, vec3 k_d, vec3 k_s, float alpha, vec3 p, vec3 eye) {
    const vec3 ambientLight = 0.5 * vec3(1.0, 1.0, 1.0);
    vec3 color = ambientLight * k_a;
    
    vec3 light1Pos = vec3(4.0 * sin(TIME/2.),
                          2.0,
                          4.0 * cos(TIME/2.));
    vec3 light1Intensity = vec3(0.4, 0.4, 0.4);
    
    color += phongContribForLight(k_d, k_s, alpha, p, eye,
                                  light1Pos,
                                  light1Intensity);
    
    return color;
}

/**
 * Return a transform matrix that will transform a ray from view space
 * to world coordinates, given the eye point, the camera target, and an up vector.
 *
 * This assumes that the center of the camera is aligned with the negative z axis in
 * view space when calculating the ray marching direction. See rayDirection.
 */
mat3 viewMatrix(vec3 eye, vec3 center, vec3 up) {
    // Based on gluLookAt man page
    vec3 f = normalize(center - eye);
    vec3 s = normalize(cross(f, up));
    vec3 u = cross(s, f);
    return mat3(s, u, -f);
}

void main() {
	vec3 viewDir = rayDirection(45.0, RENDERSIZE.xy, gl_FragCoord.xy);
    // vec3 eye = vec3(8.0, 5.0 * sin(0.2 * TIME), 7.0);
    vec3 eye = vec3(8.0, 5.0 * sin(0.2), 7.0);
    
    mat3 viewToWorld = viewMatrix(eye, vec3(0.0, 0.0, 0.0), vec3(0.0, 1.0, 0.0));
    
    vec3 worldDir = viewToWorld * viewDir;
    
    float dist = shortestDistanceToSurface(eye, worldDir, MIN_DIST, MAX_DIST);
    
    if (dist > MAX_DIST - EPSILON) {
        // Didn't hit anything
        gl_FragColor = vec4(0.0, 0.0, 0.0, 0.0);
		return;
    }
    
    // The closest point on the surface to the eyepoint along the view ray
    vec3 p = eye + dist * worldDir;
    
    vec3 K_a = (p + groundColor.rgb) / 2.0;
    vec3 K_d = K_a;
    vec3 K_s = vec3(1.0, 1.0, 1.0);
    float shininess = 10.0;
    
    vec3 color = phongIllumination(K_a, K_d, K_s, shininess, p, eye);
    
    gl_FragColor = vec4(color, 1.0);
}
