/*{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "CREDIT": "Automatically converted from https://www.shadertoy.com/view/llS3RK by Kyle273.  A simple Worley noise shader. Full tutorial at ibreakdownshaders.blogspot.com. Original shader from  http://glslsandbox.com/e#23237.0",
    "DESCRIPTION": "Worley Noise Waters",
    "IMPORTED": {
    },
    "INPUTS": [
        {
            "DEFAULT": 0.2,
            "LABEL": "Red",
            "MAX": 1,
            "MIN": 0,
            "NAME": "r",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.1,
            "LABEL": "Green",
            "MAX": 1,
            "MIN": 0,
            "NAME": "g",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.8,
            "LABEL": "Blue",
            "MAX": 1,
            "MIN": 0,
            "NAME": "b",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1,
            "LABEL": "Alpha",
            "MAX": 1,
            "MIN": 0,
            "NAME": "a",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.8,
            "LABEL": "Gradient A",
            "MAX": 3,
            "MIN": 0,
            "NAME": "gradientA",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.8,
            "LABEL": "Gradient B",
            "MAX": 3,
            "MIN": 0,
            "NAME": "gradientB",
            "TYPE": "float"
        },
        {
            "DEFAULT": 43.13311,
            "LABEL": "Noise A",
            "MAX": 100,
            "MIN": 0,
            "NAME": "noiseA",
            "TYPE": "float"
        },
        {
            "DEFAULT": 31.0011,
            "LABEL": "Noise B",
            "MAX": 100,
            "MIN": 0,
            "NAME": "noiseB",
            "TYPE": "float"
        },
        {
            "DEFAULT": 30000,
            "LABEL": "luminance",
            "MAX": 1000000,
            "MIN": 1,
            "NAME": "lum",
            "TYPE": "float"
        },
        {
            "DEFAULT": 1500.0,
            "LABEL": "Intensity",
            "MAX": 10000,
            "MIN": 10,
            "NAME": "intensity",
            "TYPE": "float"
        }
    ],
    "ISFVSN": "2"
}
*/


//Calculate the squared length of a vector
float length2(vec2 p){
    return dot(p,p);
}

//Generate some noise to scatter points.
float noise(vec2 p){
//	return fract(sin(fract(sin(p.x) * (43.13311)) + p.y) * 31.0011);
	return fract(sin(fract(sin(p.x) * (noiseA)) + p.y) * noiseB);
}

float worley(vec2 p) {
    //Set our distance to infinity
	float d = lum;
    //For the 9 surrounding grid points
	for (int xo = -1; xo <= 1; ++xo) {
		for (int yo = -1; yo <= 1; ++yo) {
            //Floor our vec2 and add an offset to create our point
			vec2 tp = floor(p) + vec2(xo, yo);
            //Calculate the minimum distance for this grid point
            //Mix in the noise value too!
			d = min(d, length2(p - tp - noise(tp)));
		}
	}
//	return 3.0*exp(-4.0*abs(2.5*d - 1.0));
	return lum*exp(-4.0*abs(2.5*d - 1.0));
}

float fworley(vec2 p) {
    //Stack noise layers 
	return sqrt(sqrt(sqrt(
		worley(p*5.0 + 0.05*TIME) *
		sqrt(worley(p * 50.0 + 0.12 + -0.1*TIME)) *
		sqrt(sqrt(worley(p * -10.0 + 0.03*TIME))))));
}
      
void main() {
	vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    //Calculate an intensity
    float t = fworley(uv * RENDERSIZE.xy / intensity);
    //Add some gradient
    t*=exp(-length2(abs(gradientA*uv - gradientB)));	
    //Make it blue!
//    gl_FragColor = vec4(t * vec3(r, g*t, pow(t, b-t)), a);
    gl_FragColor = vec4(t * vec3(r, g, b), a);
}

