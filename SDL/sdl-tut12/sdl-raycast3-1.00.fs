\ ===[ Code Addendum 02 ]============================================
\                gforth: SDL/OpenGL Graphics Part XII
\ ===================================================================
\    File Name: sdl-raycast3-1.00.fs
\      Version: 1.00
\       Author: Timothy Trussell
\         Date: 06/13/2010
\  Description: libcc interface for gforth Simple Raycaster
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-22
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\              Simple Raycast v1.00 Library for gforth
\ ===================================================================
\ This file contains the functions required to speed up the Simple
\ Raycaster by putting the core loop that draws all of the pixels
\ into a compiled C function.
\ ===================================================================
\ All of the data structures will be defined in the gforth source, so
\ anything the C code has to access will have to be passed via the
\ libcc protocols.
\ ===================================================================

\ ---[ gforth Prototypes ]-------------------------------------------
\ ray-getpixel32        ( *dst x y -- pixel )
\ ray-putpixel32        ( *dst x y pixel -- )
\ ray-draw-wall         ( x idx h top bottom *texture *dest -- )
\ ray-renderframe       ( *person *wall *flen-matrix MaxWalls
\                         *flooring *texture *dest WhichTexture -- )
\ ------------------------------------------------[End Prototypes]---

c-library sdl_raycast3

\c #include <stdlib.h>
\c #include <unistd.h>
\c #include <math.h>
\c #include <stdio.h>
\c #include <SDL/SDL.h>

\c Uint32 gpixel32( SDL_Surface *src, int x, int y )
\c {
\c     //Convert the pixels to 32 bit
\c     Uint32 *pixels = (Uint32 *)src->pixels;
\c     
\c     //Get the requested pixel
\c     return pixels[ ( y * src->w ) + x ];
\c }

\c void ppixel32( SDL_Surface *dst, int x, int y, Uint32 pixel )
\c {
\c     //Convert the pixels to 32 bit
\c     Uint32 *pixels = (Uint32 *)dst->pixels;
\c     
\c     //Set the pixel
\c     pixels[ ( y * dst->w ) + x ] = pixel;
\c }
\
\ ---[ ray_draw_wall ]-----------------------------------------------
\ This function draws a vertical 'slice' of the current wall texture
\ to the *dest surface buffer.
\
\  ray_draw_wall parameters:
\       x            - vertical slice to draw
\       idx          - index into texture array
\       h            - (float) height of slice to draw
\       top          - top line of slice
\       bottom       - bottom line of slice
\       *texture     - surface with texture pattern data
\       *dest        - dest sdl-surface to draw to
\
\c void ray_draw_wall (int x,
\c                     int idx,
\c                     float h,
\c                     int top,
\c                     int bottom,
\c                     SDL_Surface *texture,
\c                     SDL_Surface *dest) {
\c   int y;
\c   Uint32 pixel;
\c   float i=0, inc = texture->h/h;
\c
\c   if (h > dest->h)
\c     i = ((float)(h - dest->h)) / 2.0 * inc;
\
\ The i += inc provides the scaling for the texture being drawn
\ Everytime it "rolls over" (increments the 1s position) the next
\ pixel in the texture is used. Until then, the current pixel is
\ drawn.  The closer you are to a wall, the smaller the increment
\ value will be.
\
\c   for (y=top; y<bottom; y++, i += inc) {
\c     ppixel32(dest,x,y,(Uint32)gpixel32(texture,idx,(int)i));
\c   }
\c }

\c #define VISION_A M_PI/3

\c typedef struct {
\c   float x, y;
\c   float pos_inc, slide_inc;
\c   float dir, dir_inc;
\c } Person;

\c typedef struct {
\c   float ax, ay, bx, by;
\c   float coef_x, coef_y, aconst;
\c   int walltype;
\c } Wall;

\c renderframe (Person *person,
\c              Wall *wall,
\c              Float flen_matrix[],
\c              int MaxWalls,
\c              SDL_Surface *flooring,
\c              SDL_Surface *texture,
\c              SDL_Surface *dest) {

\c   float count, count_temp,dir,f_length;
\c   float floor_x, floor_y,x,y,x_add, y_add;
\c   int i,n,screen_x,screen_y,fx,fy;
\c   int wall_height, wall_bottom, wall_top, wall_index;
\c   unsigned char r;
\c   Uint32 bright,shaded;             // ceiling/floor 24-bit pixels

\c   if (SDL_MUSTLOCK(dest))
\c     SDL_LockSurface(dest);

\c   for (screen_x=0; screen_x<dest->w; screen_x++) {
\c     dir = person->dir + (screen_x-dest->w/2)*VISION_A/dest->w;
\c     x_add = cos(dir);
\c     y_add = sin(dir);
\c     n = 0;
\c     count = 20000;
\ 
\      Process the walls
\
\c     for (i = 0; i < MaxWalls; i++) {
\c       Wall *w = &wall[i];
\c       if (w->coef_x*x_add+w->coef_y*y_add != 0) {
\c         count_temp = -(w->coef_x*person->x + 
\c                        w->coef_y*person->y + 
\c                        w->aconst) / 
\c                       (w->coef_x*x_add+w->coef_y*y_add);
\c         x = person->x + x_add * count_temp;
\c         y = person->y + y_add * count_temp;
\c         if (count_temp > 0 && count_temp < count
\c             && x >= w->ax && x <= w->bx
\c             && y >= w->ay && y <= w->by) {
\c           count = count_temp;
\c           n = i;
\c         }
\c       }
\c     }

\c     x = person->x + x_add * count;
\c     y = person->y + y_add * count;

\c     if (wall[n].coef_x != 0)
\c       wall_index = (int)(y / wall[n].coef_x * 200);
\c     else
\c       wall_index = (int)(x / wall[n].coef_y * 200);

\c     if (wall_index<0)
\c       wall_index *= -1;
\ 
\    The generated texture image width is 20, while the Wolf3D
\    image width is 64, so select which to MOD with based on the
\    surface->w field.
\
\c     if (texture->w==20)
\c       wall_index = wall_index%20;
\c     else
\c       wall_index = wall_index%64;
\
\ Add offset to the texture to be used
\
\c     wall_index+=wall[n].walltype*64;

\c     wall_height = dest->h/count;
\c     wall_top    = (dest->h-wall_height)/2;
\c     wall_bottom = (dest->h+wall_height)/2;

\c     if (wall_top < 0)
\c       wall_top = 0;
\c     if (wall_bottom > dest->h)
\c       wall_bottom = dest->h;
\ 
\      Draw the ceiling/floor slice 
\
\c     for (screen_y=0; screen_y<=wall_top; screen_y++) {
\c       f_length = flen_matrix[screen_y];
\c       floor_x = f_length * x_add - person->x + 10000;
\c       floor_y = f_length * y_add - person->y + 10000;
\c       fx = (int)(floor_x*80)%20;
\c       fy = (int)(floor_y*80)%20;
\c       r = (int)gpixel32(flooring,fx,fy);
\c       if (f_length<0)
\c         f_length *= -1;
\c       f_length += 8;

\c       r = (int)(r/f_length)*4;
\c       shaded = (r<<16)|(r<<8)|(r);     // make into a 24-bit pixel
\c       bright = shaded<<1;               // make the pixel brighter

\c       ppixel32(dest,screen_x,screen_y,bright);          // ceiling
\c       ppixel32(dest,screen_x,dest->h-screen_y-1,shaded);  // floor
\c     }
\
\      Draw the wall slice 
\ 
\c     ray_draw_wall (screen_x,
\c                    wall_index,
\c                    wall_height,
\c                    wall_top,
\c                    wall_bottom,
\c                    texture,
\c                    dest);
\c   }
\c   if (SDL_MUSTLOCK(dest))
\c     SDL_UnlockSurface(dest);
\c }

c-function ray-getpixel32       gpixel32                   a n n -- n
c-function ray-putpixel32       ppixel32              a n n n -- void
c-function ray-draw-wall        ray_draw_wall   n n r n n a a -- void
c-function ray-renderframe      renderframe     a a a n a a a -- void

end-c-library

\ ========================================[End sdl-raycast3 libcc]===

