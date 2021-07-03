/*{
    "CATEGORIES": [
        "52",
        "52.27"
    ],
    "DESCRIPTION": "Inspired by https://www.shadertoy.com/view/WsV3D1 by atzmael. ",
    "IMPORTED": {
    },
    "INPUTS": [
        {
            "DEFAULT": [
                0,
                0,
                0,
                1
            ],
            "LABEL": "Background Color",
            "NAME": "backgroundColor",
            "TYPE": "color"
        },
		{
            "DEFAULT": [
                0.975,
                0,
                0,
                1
            ],
            "LABEL": "Circle 1 Color",
            "NAME": "circle1Color",
            "TYPE": "color"
        },
        {
            "DEFAULT": [
                0.99,
                0.493,
                0,
                1
            ],
            "LABEL": "Circle 2 Color",
            "NAME": "circle2Color",
            "TYPE": "color"
        },
        {
            "DEFAULT": [
                0.955,
                0.955,
                0,
                1
            ],
            "LABEL": "Circle 3 Color",
            "NAME": "circle3Color",
            "TYPE": "color"
        },
        {
            "DEFAULT": [
                0,
                0.985,
                0,
                1
            ],
            "LABEL": "Circle 4 Color",
            "NAME": "circle4Color",
            "TYPE": "color"
        },
        {
            "DEFAULT": [
                0,
                0,
                0.965,
                1
            ],
            "LABEL": "Circle 5 Color",
            "NAME": "circle5Color",
            "TYPE": "color"
        },
        {
            "DEFAULT": [
                0.694,
                0,
                0.99,
                1
            ],
            "LABEL": "Circle 6 Color",
            "NAME": "circle6Color",
            "TYPE": "color"
        },
        {
            "DEFAULT": [
                0.571,
                0,
                0.99,
                1
            ],
            "LABEL": "Circle 7 Color",
            "NAME": "circle7Color",
            "TYPE": "color"
        },
        {
            "DEFAULT": 125,
            "LABEL": "Beats per Minute",
            "MAX": 240,
            "MIN": 1,
            "NAME": "bpm",
            "TYPE": "float"
        },
        {
            "DEFAULT": [
                0.01,
                0.01
            ],
            "LABEL": "Circle 1 Min Max",
            "MAX": [
                1,
                1
            ],
            "MIN": [
                0.01,
                0.01
            ],
            "NAME": "circle1MinMax",
            "TYPE": "point2D"
        },
        {
            "DEFAULT": [
                0.01,
                0.01
            ],
            "LABEL": "Circle 2 Min Max",
            "MAX": [
                1,
                1
            ],
            "MIN": [
                0.01,
                0.01
            ],
            "NAME": "circle2MinMax",
            "TYPE": "point2D"
        },
        {
            "DEFAULT": [
                0.01,
                0.01
            ],
            "LABEL": "Circle 3 Min Max",
            "MAX": [
                1,
                1
            ],
            "MIN": [
                0.01,
                0.01
            ],
            "NAME": "circle3MinMax",
            "TYPE": "point2D"
        },
        {
            "DEFAULT": [
                0.01,
                0.01
            ],
            "LABEL": "Circle 4 Min Max",
            "MAX": [
                1,
                1
            ],
            "MIN": [
                0.01,
                0.01
            ],
            "NAME": "circle4MinMax",
            "TYPE": "point2D"
        },
        {
            "DEFAULT": [
                0.01,
                0.01
            ],
            "LABEL": "Circle 5 Min Max",
            "MAX": [
                1,
                1
            ],
            "MIN": [
                0.01,
                0.01
            ],
            "NAME": "circle5MinMax",
            "TYPE": "point2D"
        },
        {
            "DEFAULT": [
                0.01,
                0.01
            ],
            "LABEL": "Circle 6 Min Max",
            "MAX": [
                1,
                1
            ],
            "MIN": [
                0.01,
                0.01
            ],
            "NAME": "circle6MinMax",
            "TYPE": "point2D"
        },
        {
            "DEFAULT": [
                0.01,
                0.01
            ],
            "LABEL": "Circle 7 Min Max",
            "MAX": [
                1,
                1
            ],
            "MIN": [
                0.01,
                0.01
            ],
            "NAME": "circle7MinMax",
            "TYPE": "point2D"
        },
        {
            "DEFAULT": [
                0.5,
                0.5
            ],
            "LABEL": "Shift Factor X",
            "MAX": [
                1,
                1
            ],
            "MIN": [
                0.01,
                0.01
            ],
            "NAME": "shiftFactorX",
            "TYPE": "point2D"
        },
        {
            "DEFAULT": [
                0.5,
                0.5
            ],
            "LABEL": "Shift Factor Y",
            "MAX": [
                1,
                1
            ],
            "MIN": [
                0.01,
                0.01
            ],
            "NAME": "shiftFactorY",
            "TYPE": "point2D"
        },
        {
            "DEFAULT": 1,
            "LABEL": "alpha",
            "MAX": 1,
            "MIN": 0,
            "NAME": "alpha",
            "TYPE": "float"
        }
    ],
    "ISFVSN": "2"
}
*/


float circle(vec2 st, float pct, float minLimit, float maxLimit) {
  return  smoothstep( pct - minLimit, pct, distance(st, vec2(shiftFactorX.x, shiftFactorY.y))) -
          smoothstep( pct, pct + maxLimit, distance(st, vec2(shiftFactorY.x, shiftFactorX.y)));
}

float fillCircle(vec2 st, float pct){
  return  step(0., distance(st,vec2(0.5))) -
          step( pct, distance(st,vec2(0.5)));
}

float map(float value, float min1, float max1, float min2, float max2) {
  return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
}

void main() {
    vec2 st = (gl_FragCoord.xy-.5*RENDERSIZE.xy)/RENDERSIZE.y+.5;

    float bbpm = 4.;  // beats per measure
    float spm = (bbpm*60./bpm)/4.; // seconds per measure

    float time = TIME;

    float drawCircle1 = map(sin(TIME / spm), -1., 1., 0.40, 0.45);
    float drawCircle2 = map(sin(TIME / spm), -1., 1., 0.35, 0.39);
    float drawCircle3 = map(sin(TIME / spm), -1., 1., 0.30, 0.34);
    float drawCircle4 = map(sin(TIME / spm), -1., 1., 0.25, 0.29);
    float drawCircle5 = map(sin(TIME / spm), -1., 1., 0.20, 0.24);
    float drawCircle6 = map(sin(TIME / spm), -1., 1., 0.15, 0.19);
    float drawCircle7 = map(sin(TIME / spm), -1., 1., 0.10, 0.14);
    
    vec3 circle1 = vec3(circle(st, drawCircle1, circle1MinMax.x, circle1MinMax.y));
    vec3 circle2 = vec3(circle(st, drawCircle2, circle2MinMax.x, circle2MinMax.y));
    vec3 circle3 = vec3(circle(st, drawCircle3, circle3MinMax.x, circle3MinMax.y));
    vec3 circle4 = vec3(circle(st, drawCircle4, circle4MinMax.x, circle4MinMax.y));
    vec3 circle5 = vec3(circle(st, drawCircle5, circle5MinMax.x, circle5MinMax.y));
    vec3 circle6 = vec3(circle(st, drawCircle6, circle6MinMax.x, circle6MinMax.y));
    vec3 circle7 = vec3(circle(st, drawCircle7, circle7MinMax.x, circle7MinMax.y));

    vec4 color =  
        smoothstep(0.0, 1.0, vec4(circle1, alpha) * mix(circle1Color, circle2Color, 0.25)) +
        smoothstep(0.0, 1.0, vec4(circle2, alpha) * mix(circle2Color, circle3Color, 0.25)) +
        smoothstep(0.0, 1.0, vec4(circle3, alpha) * mix(circle3Color, circle4Color, 0.25)) +
        smoothstep(0.0, 1.0, vec4(circle4, alpha) * mix(circle4Color, circle5Color, 0.25)) +
        smoothstep(0.0, 1.0, vec4(circle5, alpha) * mix(circle5Color, circle6Color, 0.25)) +
        smoothstep(0.0, 1.0, vec4(circle6, alpha) * mix(circle6Color, circle7Color, 0.25)) + 
        smoothstep(0.0, 1.0, vec4(circle7, alpha) * circle7Color);

	color += backgroundColor;

    gl_FragColor = color;
}
