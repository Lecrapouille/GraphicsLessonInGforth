\ ===================================================================
\           File: opengllib-1.22.fs
\         Author: Jens Schneider
\  Linux Version: Ti Leggett
\ gForth Version: Timothy Trussell, 08/06/2010
\    Description: Bump mapping (extensions)
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-24
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 22
\ ===================================================================
\ This code was created by Jeff Molofee '99 
\ (ported to Linux/SDL by Ti Leggett '01)
\ Visit Jeff at http://nehe.gamedev.net/
\ ===================================================================

\ ---[ UseLibUtil ]--------------------------------------------------
\ Conditional compilation of the libcc interfaces:
\ -- Set to 1 if you use the LibUtil script to copy the files to
\    the ~/.gforth directory.
\ -- Set to 0 to use the files from the Lesson directory (PWD).

1 constant UseLibUtil

UseLibUtil [if]
  require ~/.gforth/opengl-libs/mini-opengl-current.fs
  require ~/.gforth/opengl-libs/mini-sdl-current.fs
  require ~/.gforth/opengl-libs/sdlkeysym.fs
[else]
  require mini-opengl-1.22.fs
  require mini-sdl-1.01.fs
  require sdlkeysym.fs
[then]

\ ---[ Prototype Listing ]-------------------------------------------
\ : IsInString                  ( *str len -- boolean )
\ : InitMultiTexture            ( -- boolean )
\ : Generate-Texture            { *src -- }
\ : Generate-RGBA8-Texture      { *src *data -- }
\ : Generate-MipMapped-Texture  { *src -- 
\ : Load-Image                  ( str len ndx -- boolean )
\ : Flip-Image                  ( *src -- )
\ : LoadGLTextures              ( -- boolean )
\ : Set-Quad                    ( f: x f: y f: z hi lo -- )
\ : DoCube                      ( -- )
\ : HandleKeyPress              ( &event -- )
\ : HandleKeyRelease            ( &event -- )
\ : Read-Extensions             ( -- )
\ : InitLights                  ( -- )
\ : InitGL                      ( -- boolean )
\ : VMatMult                    { *m *v -- }
\ : SetupBumps                  { *n *c *l *s *t -- }
\ : DoLogo                      ( -- )
\ : SetFaceMesh1                ( r1 r2 r3 r4 f5 r6 r7 r8 r9 hi lo --
\ : DoMesh1TexelUnits           ( -- boolean )
\ : SetFaceMesh2                ( r1 r2 r3 r4 f5 r6 r7 r8 r9 hi lo --
\ : DoMesh2TexelUnits           ( -- boolean )
\ : DoMeshNoBumps               ( -- boolean )
\ : ShutDown                    ( -- )
\ : Reset-FPS-Counter           ( -- )
\ : Display-FPS                 ( -- )
\ : DrawGLScene                 ( -- boolean )
\ ------------------------------------------------[End Prototypes]---

\ ---[ Variables ]---------------------------------------------------

3 constant NumTextures                    \ number of textures to use
3 constant NumBumps                  \ number of bump textures to use
3 constant NumInvBumps       \ number of inverse bump textures to use
6 constant NumImages                  \ number of images being loaded

\ Maximum Emboss-Translate. Increase To Get Higher Immersion
0.01e fconstant Max-Emboss

FALSE value emboss                                   \ boolean toggle
TRUE value bumps                                     \ boolean toggle

1 value filter                   \ which filter to use, range: [0..2]

variable glLogo                              \ handle for OpenGL Logo
variable multiLogo             \ handle For Multitexture-Enabled-Logo

fvariable xrot                                           \ x rotation
fvariable yrot                                           \ y rotation
fvariable zrot                                           \ z rotation
fvariable xspeed                                            \ x speed
fvariable yspeed                                            \ y speed
fvariable zdepth                                  \ depth into screen

\ Allot space for texture pointers and init the memory
create texture  here NumTextures cells dup allot 0 fill
create bump     here NumBumps    cells dup allot 0 fill
create invbump  here NumInvBumps cells dup allot 0 fill
create teximage here NumImages   cells dup allot 0 fill

\ ---[ Array Index Functions ]---------------------------------------
\ Index functions to access the arrays

: texture-ndx  ( n -- *tex[n] ) NumTextures MOD cells texture + ;
: bump-ndx     ( n -- *tex[n] ) NumBumps    MOD cells bump + ;
: invbump-ndx  ( n -- *tex[n] ) NumInvBumps MOD cells invbump + ;
: teximage-ndx ( n -- *tex[n] ) cells teximage + ;

\ Returns a pointer to the nth element of an array of floats/sfloats
: farray-ndx  ( *array n -- *array[n] ) floats + ;
: sfarray-ndx ( *array n -- *array[n] ) sfloats + ;

\ ---[ Light Values ]------------------------------------------------
\ The following three arrays are RGBA color shadings.

\ These light tables are passed by address to gl-light-fv, not value,
\ so they must be stored as 32-bit floats, not gforth 64-bit floats.

\ Ambient Light Values
create LightAmbient[]   0.2e SF, 0.2e SF, 0.2e SF, 1e SF,

\ Diffuse Light Values (white)
create LightDiffuse[]   1e SF, 1e SF, 1e SF, 1e SF,

\ Light Position
create LightPosition[]  0e SF, 0e SF, 2e SF, 1e SF,

\ Grays
create Gray[]           0.5e SF, 0.5e SF, 0.5e SF, 1e SF,

\ Data we'll use to generate our cube
\ These are passed by value, not address, so can stay 64-bits

struct
  float% field .tx
  float% field .ty
  float% field .x
  float% field .y
  float% field .z
end-struct vertice%

create data[]
0e F, 0e F,     -1e F, -1e F,  1e F,                     \ Front Face
1e F, 0e F,      1e F, -1e F,  1e F,
1e F, 1e F,      1e F,  1e F,  1e F,
0e F, 1e F,     -1e F,  1e F,  1e F,

1e F, 0e F,     -1e F, -1e F, -1e F,                      \ Back Face
1e F, 1e F,     -1e F,  1e F, -1e F,
0e F, 1e F,      1e F,  1e F, -1e F,
0e F, 0e F,      1e F, -1e F, -1e F,

0e F, 1e F,     -1e F,  1e F, -1e F,                       \ Top Face
0e F, 0e F,     -1e F,  1e F,  1e F,
1e F, 0e F,      1e F,  1e F,  1e F,
1e F, 1e F,      1e F,  1e F, -1e F,

1e F, 1e F,     -1e F, -1e F, -1e F,                    \ Bottom Face
0e F, 1e F,      1e F, -1e F, -1e F,
0e F, 0e F,      1e F, -1e F,  1e F,
1e F, 0e F,     -1e F, -1e F,  1e F,

1e F, 0e F,      1e F, -1e F, -1e F,                     \ Right Face
1e F, 1e F,      1e F,  1e F, -1e F,
0e F, 1e F,      1e F,  1e F,  1e F,
0e F, 0e F,      1e F, -1e F,  1e F,

0e F, 0e F,     -1e F, -1e F, -1e F,                      \ Left Face
1e F, 0e F,     -1e F, -1e F,  1e F,
1e F, 1e F,     -1e F,  1e F,  1e F,
0e F, 1e F,     -1e F,  1e F, -1e F,

: data-ndx ( n -- data[n] ) vertice% nip * data[] + ;

\ Prepare for GL_ARB_multitexture
\ Used To Disable ARB Extensions Entirely
TRUE value ARB_ENABLE

\ Set to TRUE to see your extensions at start-up
TRUE value EXT_INFO

\ Characters For Extension-Strings
10240 constant MAX_EXTENSION_SPACE

\ Maximum Characters In One Extension-String
256 constant MAX_EXTENSION_LENGTH

\ Flag Indicating Whether Multitexturing Is Supported
\ -- this is set in InitGL
FALSE value MultiTextureSupported

\ Use It If It Is Supported?
TRUE value UseMultiTexture

\ Number Of Texel-Pipelines. This Is At Least 1.
variable MaxTexelUnits

\ ---[ Variable Initializations ]------------------------------------

0e xrot F!
0e yrot F!
0e zrot F!
-5e zdepth F!
1 MaxTexelUnits !

\ ---[ Check for GL Extensions ]-------------------------------------

\ ---[ StrLen ]------------------------------------------------------
\ Returns the length of a NULL terminated string

: strlen { *str -- len }
  0 begin *str over + C@ 0= if 1 else 1+ 0 then until
;

\ ---[ IsInString ]--------------------------------------------------
\ Searches the OpenGL Extension data string to find the extension.

: IsInString ( *str len -- boolean )
  FALSE { *str len result -- boolean }
  GL_EXTENSIONS gl-get-string dup 0<> if    \ if ==0 we have an error
    dup strlen *str len search if
      TRUE to result
    then
    2drop                  \ lose the string pointer <search> returns
  then
  result
;

\ ---[ InitMultiTexture ]--------------------------------------------
\ Determines if ARB_multitexture is available

: InitMultiTexture ( -- boolean )
  s" GL_ARB_multitexture"        IsInString
  s" GL_EXT_texture_env_combine" IsInString AND ARB_Enable AND if
    cr ." The GL_ARB_multitexture extension will be used." cr cr
    GL_MAX_TEXTURE_UNITS_ARB MaxTexelUnits gl-get-integer-v
    TRUE                                               \ return value
  else
    cr ." The GL_ARB_multitexture extension not supported." cr cr
    \ We Can't Use It If It Isn't Supported!
    FALSE to UseMultiTexture
    FALSE                                              \ return value
  then
;
    
\ ---[ LoadGLTextures ]----------------------------------------------
\ function to load in bitmap as a GL texture

: Generate-Texture { *src -- }
  GL_TEXTURE_2D 0 GL_RGB8
  *src sdl-surface-w @                       \ width of texture image
  *src sdl-surface-h @                      \ height of texture image
  0 GL_BGR                                \ pixel mapping orientation
  GL_UNSIGNED_BYTE
  *src sdl-surface-pixels @                 \ address of texture data
  gl-tex-image-2d                               \ finally generate it
;

: Generate-RGBA8-Texture { *src *data -- }
  GL_TEXTURE_2D 0 GL_RGBA8
  *src sdl-surface-w @                       \ width of texture image
  *src sdl-surface-h @                      \ height of texture image
  0 GL_RGBA                               \ pixel mapping orientation
  GL_UNSIGNED_BYTE
  *data                                     \ address of texture data
  gl-tex-image-2d                               \ finally generate it
;

: Generate-MipMapped-Texture { *src -- 
  GL_TEXTURE_2D GL_RGB8
  *src sdl-surface-w @                       \ width of texture image
  *src sdl-surface-h @                      \ height of texture image
  GL_BGR                                  \ pixel mapping orientation
  GL_UNSIGNED_BYTE
  *src sdl-surface-pixels @                 \ address of texture data
  glu-build-2d-mipmaps                          \ finally generate it
;

\ ---[ Load-Image ]--------------------------------------------------
\ Attempt to load the texture images into SDL surfaces, saving the
\ result into the teximage[] array; Return TRUE if result from
\ sdl-loadbmp is <> 0; else return FALSE

: Load-Image ( str len ndx -- boolean )
  >R zstring sdl-loadbmp dup R> teximage-ndx ! 0<>
;

\ ---[ Flip-Image ]--------------------------------------------------
\ Reverses the line ordering of a .BMP image.
\ Requires the handle of the SDL surface to be flipped.

: Flip-Image ( *src -- )
  0 0 0 0 { *src *tdata *timage _b/l _height -- }   \ local variables

  \ Set *tdata to the pixel data in the source surface
  *src sdl-surface-pixels @ to *tdata

  \ Set *timage to the next paragraph boundary above <here>
  \ This does NOT allot memory in the dictionary - it just uses some
  \ of it above <here> as a temporary buffer space.

  here 256 mod 256 swap - here + to *timage

  \ Set the sizes to be used

  *src sdl-surface-pitch sw@ to _b/l          \ bytes/line of surface
  *src sdl-surface-h @ to _height     \ number of rows in the surface

  \ Copy/flip the pixel data to the temp buffer space

  _height 0 do
    *tdata i _b/l * +                                \ source line[i]
    *timage _height i - _b/l * +                     \ dest line[h-i]
    _b/l                                             \ length to move
    cmove
  loop

  *timage *tdata _b/l _height * cmove       \ copy new image over old
;

: LoadGLTextures ( -- status )
  0 0 0 { *tdata *timage _status -- status }
\ create local variable for storing return flag and SDL surfaces
  teximage NumImages cells 0 fill     \ Init array of surface handles
  s" data/base.bmp0"           0 Load-Image 
  s" data/bump.bmp0"           1 Load-Image AND
  s" data/opengl_alpha.bmp0"   2 Load-Image AND
  s" data/opengl.bmp0"         3 Load-Image AND
  s" data/multi_on_alpha.bmp0" 4 Load-Image AND
  s" data/multi_on.bmp0"       5 Load-Image AND if
    \ All the images loaded, so continue
    TRUE to _status                                \ set return value

    NumTextures texture gl-gen-textures         \ create the textures

    \ ---[ Process base.bmp ]---
    
    GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture  \ load texture [0]
    \ Nearest filtering
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_NEAREST gl-tex-parameter-i
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_NEAREST gl-tex-parameter-i
    \ Generate the texture
    0 teximage-ndx @ Generate-Texture

    \ Create Linear Filtered Texture
    GL_TEXTURE_2D 1 texture-ndx @ gl-bind-texture  \ load texture [1]
    \ Linear filtering
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR gl-tex-parameter-i
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR gl-tex-parameter-i
    \ Generate the texture
    0 teximage-ndx @ Generate-Texture
    
    \ Create MipMapped Texture
    GL_TEXTURE_2D 2 texture-ndx @ gl-bind-texture  \ load texture [2]
    \ Mipmap filtering
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR gl-tex-parameter-i
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR_MIPMAP_NEAREST
    gl-tex-parameter-i
    \ Generate the texture
    0 teximage-ndx @ Generate-MipMapped-Texture

    \ ---[ Process bump.bmp ]---

    \ Scale RGB by 50%    
    GL_RED_SCALE 0.5e gl-pixel-transfer-f
    GL_GREEN_SCALE 0.5e gl-pixel-transfer-f
    GL_BLUE_SCALE 0.5e gl-pixel-transfer-f
    
    \ Specify not to wrap the texture
    GL_TEXTURE_2D GL_TEXTURE_WRAP_S GL_CLAMP gl-tex-parameter-i
    GL_TEXTURE_2D GL_TEXTURE_WRAP_T GL_CLAMP gl-tex-parameter-i
    
    NumBumps bump gl-gen-textures         \ create the bump textures
    
    GL_TEXTURE_2D 0 bump-ndx @ gl-bind-texture         \ load bump[0]
    \ Nearest filtering
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_NEAREST gl-tex-parameter-i
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_NEAREST gl-tex-parameter-i
    \ Generate the texture
    1 teximage-ndx @ Generate-Texture
    
    \ Create Linear Filtered Texture
    GL_TEXTURE_2D 1 bump-ndx @ gl-bind-texture         \ load bump[1]
    \ Linear filtering
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR gl-tex-parameter-i
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR gl-tex-parameter-i
    \ Generate the texture
    1 teximage-ndx @ Generate-Texture
    
    \ Create MipMapped Texture
    GL_TEXTURE_2D 2 bump-ndx @ gl-bind-texture         \ load bump[2]
    \ Mipmap filtering
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR gl-tex-parameter-i
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR_MIPMAP_NEAREST
    gl-tex-parameter-i
    \ Generate the texture
    1 teximage-ndx @ Generate-MipMapped-Texture

    \ Invert the Bumpmap
    \ bump.bmp is a 256x256x24b image.
    \ We are subtracting each color element from 255 to invert it.
    
    1 teximage-ndx @ >R 
    R@ sdl-surface-pixels @ to *tdata
    3 R@ sdl-surface-w @ * R> sdl-surface-h @ * 0 do
      255 *tdata i + C@ - *tdata i + C!
    loop
    
    \ ---[ Process inverted bump.bmp ]---
    
    NumInvBumps invbump gl-gen-textures \ create the invbump textures
    
    GL_TEXTURE_2D 0 invbump-ndx @ gl-bind-texture   \ load invbump[0]
    \ Nearest filtering
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_NEAREST gl-tex-parameter-i
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_NEAREST gl-tex-parameter-i
    \ Generate the texture
    1 teximage-ndx @ Generate-Texture
    
    \ Create Linear Filtered Texture
    GL_TEXTURE_2D 1 invbump-ndx @ gl-bind-texture   \ load invbump[1]
    \ Linear filtering
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR gl-tex-parameter-i
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR gl-tex-parameter-i
    \ Generate the texture
    1 teximage-ndx @ Generate-Texture
    
    \ Create MipMapped Texture
    GL_TEXTURE_2D 2 invbump-ndx @ gl-bind-texture   \ load invbump[2]
    \ Mipmap filtering
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR gl-tex-parameter-i
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR_MIPMAP_NEAREST
    gl-tex-parameter-i
    \ Generate the texture
    1 teximage-ndx @ Generate-MipMapped-Texture

    \ ---[ Process opengl_alpha.bmp ]---
    
    \ Expand the 24bpp image to 32bpp, giving it an Alpha element
    
    \ Set *timage to the next paragraph boundary above <here>
    here 256 mod 256 swap - here + to *timage

    \ Get the handle for the opengl_alpha.bmp surface
    2 teximage-ndx @ >R
    \ Set *tdata to point to the source image pixel data
    R@ sdl-surface-pixels @ to *tdata
    \ Set the alpha element for the RGBA8-texture to the RED pixel
    \ from the opengl_alpha.bmp image
    R@ sdl-surface-w @ R> sdl-surface-h @ * 0 do
      *tdata i 3 * + C@                        \ get source red pixel
      *timage i 4 * 3 + + C!                    \ set dest alpha byte
    loop

    \ Now copy the pixel data from the opengl.bmp image

    \ Get the handle for the opengl.bmp surface
    3 teximage-ndx @ >R
    \ Set *tdata to the pixel data of the opengl.bmp surface
    R@ sdl-surface-pixels @ to *tdata
    \ Copy three pixel elements during each loop pass
    R@ sdl-surface-w @ R> sdl-surface-h @ * 0 do
      *tdata i 3 * + *timage i 4 * + 3 cmove
    loop    

    \ ---[ Process opengl.bmp ]---

    1 glLogo gl-gen-textures              \ create the gllogo texture
    \ Create linear filtered RGBA8 texture
    GL_TEXTURE_2D glLogo @ gl-bind-texture
    \ Linear filtering
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR gl-tex-parameter-i
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR gl-tex-parameter-i
    \ Generate the texture
    3 teximage-ndx @ *timage Generate-RGBA8-Texture    

    \ We need to flip the next two images, as .BMPs are reversed

    4 teximage-ndx @ Flip-Image                  \ multi_on_alpha.bmp
    5 teximage-ndx @ Flip-Image                        \ multi_on.bmp

    \ ---[ Process multi_on_alpha.bmp ]---

    \ Expand the 24bpp image to 32bpp, giving it an Alpha element

    \ Get the handle for the multi_on_alpha.bmp surface
    4 teximage-ndx @ >R
    \ Set *tdata to point to the source image pixel data
    R@ sdl-surface-pixels @ to *tdata
    \ Set the alpha element for the RGBA8-texture to the RED pixel
    \ from the opengl_alpha.bmp image
    R@ sdl-surface-w @ R> sdl-surface-h @ * 0 do
      *tdata i 3 * + C@                        \ get source red pixel
      *timage i 4 * 3 + + C!                    \ set dest alpha byte
    loop

    \ ---[ Process multi_on.bmp ]---

    \ Get the handle for the multi_on.bmp surface
    5 teximage-ndx @ >R
    \ Set *tdata to the pixel data of the opengl.bmp surface
    R@ sdl-surface-pixels @ to *tdata

    \ Now copy the 24-bpp data to the 32-bpp data structure
    \ -- Copy three pixel elements during each loop pass
    R@ sdl-surface-w @ R> sdl-surface-h @ * 0 do
      *tdata i 3 * + *timage i 4 * + 3 cmove
    loop    

    1 MultiLogo gl-gen-textures        \ create the MultiLogo texture
    \ Create linear filtered RGBA8 texture
    GL_TEXTURE_2D MultiLogo @ gl-bind-texture
    \ Linear filtering
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR gl-tex-parameter-i
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR gl-tex-parameter-i
    \ Generate the texture
    5 teximage-ndx @ *timage Generate-RGBA8-Texture    

  else
    \ At least one of the images did not load, so exit
    cr ." Error: texture image could not be loaded!" cr
  then
  \ Free the image surfaces that were created
  NumImages 0 do
    i teximage-ndx @ dup 0<> if sdl-freesurface else drop then
  loop
  _status                      \ exit with return value: 0=fail;-1=ok
;

\ ---[ doCube ]------------------------------------------------------
\ Initializes GL Quads from the data[] array
\ Remember: gforth has separate integer and floating point stacks...

: Set-Quad ( f: x f: y f: z hi lo -- )
  gl-normal-3f
  do
    i data-ndx >R
    R@ .tx F@ R@ .ty F@ gl-tex-coord-2f
    R@ .x F@ R@ .y F@ R> .z F@ gl-vertex-3f
  loop
;

\ function to draw a cube
: DoCube ( -- )
  GL_QUADS gl-begin
     0e  0e  1e    4  0 Set-Quad                         \ Front face
     0e  0e -1e    8  4 Set-Quad                          \ Back face
     0e  1e  0e   12  8 Set-Quad                           \ Top face   
     0e -1e  0e   16 12 Set-Quad                        \ Bottom face
     1e  0e  0e   20 16 Set-Quad                         \ Right face
    -1e  0e  0e   24 20 Set-Quad                          \ Left face
  gl-end
;

\ ---[ Keyboard Flags ]----------------------------------------------
\ Flags needed to prevent constant toggling if the keys that they
\ represent are held down during program operation.
\ By checking to see if the specific flag is already set, we can then
\ choose to ignore the current keypress event for that key.

0 value key-ESC
0 value key-F1
0 value key-b
0 value key-e
0 value key-f
0 value key-m
0 value key-PgDn
0 value key-PgUp
0 value key-Up
0 value key-Dn
0 value key-Right
0 value key-Left

\ ---[ HandleKeyPress ]----------------------------------------------
\ function to handle key press events:
\   ESC      exits the program
\   F1       toggles between fullscreen and windowed modes
\   b        toggles bumps
\   e        toggles embossing
\   f        cycles through filters
\   m        toggles multitextured support
\   PgUp     zooms into the screen
\   PgDn     zooms out of the screen
\   Up       increases x rotation speed
\   Dn       decreases x rotation speed
\   Right    increases y rotation speed
\   Left     decreases y rotation speed

: HandleKeyPress ( &event -- )
  sdl-keyboard-event-keysym sdl-keysym-sym uw@
  case
    SDLK_ESCAPE   of TRUE to opengl-exit-flag endof
    SDLK_F1       of key-F1 FALSE = if      \ skip if being held down
                       screen sdl-wm-togglefullscreen drop
                       TRUE to key-F1          \ set key pressed flag
                     then
                  endof
    SDLK_b        of key-b FALSE = if       \ skip if being held down
                       bumps if 0 else 1 then to bumps
                       TRUE to key-b           \ set key pressed flag
                     then
                  endof
    SDLK_e        of key-e FALSE = if       \ skip if being held down
                       emboss if 0 else 1 then to emboss
                       TRUE to key-e           \ set key pressed flag
                     then
                  endof
    SDLK_f        of key-f FALSE = if       \ skip if being held down
                       filter 1+ 3 MOD to filter
                       TRUE to key-f           \ set key pressed flag
                     then
                  endof
    SDLK_m        of key-m FALSE = if       \ skip if being held down
                       UseMultiTexture if 0 else 1 then
                       MultiTextureSupported AND to UseMultiTexture
                       TRUE to key-m           \ set key pressed flag
                     then
                  endof
    SDLK_PAGEUP   of zdepth F@ 0.02e F- zdepth F! endof
    SDLK_PAGEDOWN of zdepth F@ 0.02e F+ zdepth F! endof
    SDLK_UP       of xspeed F@ 0.01e F- xspeed F! endof
    SDLK_DOWN     of xspeed F@ 0.01e F+ xspeed F! endof
    SDLK_RIGHT    of yspeed F@ 0.01e F- yspeed F! endof
    SDLK_LEFT     of yspeed F@ 0.01e F+ yspeed F! endof
  endcase
;

\ ---[ HandleKeyRelease ]--------------------------------------------
\ Function to handle key release events
\ I have added all of the key flags, even though not all are being
\ accessed in the HandleKeyPress function.

: HandleKeyRelease ( &event -- )
  sdl-keyboard-event-keysym sdl-keysym-sym uw@
  case
    SDLK_ESCAPE   of FALSE to key-ESC   endof
    SDLK_F1       of FALSE to key-F1    endof
    SDLK_b        of FALSE to key-b     endof
    SDLK_e        of FALSE to key-e     endof
    SDLK_f        of FALSE to key-f     endof
    SDLK_m        of FALSE to key-m     endof
    SDLK_PAGEUP   of FALSE to key-PgUp  endof
    SDLK_PAGEDOWN of FALSE to key-PgDn  endof
    SDLK_UP       of FALSE to key-Up    endof
    SDLK_DOWN     of FALSE to key-Dn    endof
    SDLK_RIGHT    of FALSE to key-Right endof
    SDLK_LEFT     of FALSE to key-Left  endof
  endcase
;

\ ---[ InitLights ]--------------------------------------------------
\ Initialize the lights

: InitLights ( -- )
  GL_LIGHT1 GL_AMBIENT  LightAmbient[]  gl-light-fv
  GL_LIGHT1 GL_DIFFUSE  LightDiffuse[]  gl-light-fv
  GL_LIGHT1 GL_POSITION LightPosition[] gl-light-fv
  GL_LIGHT1 gl-enable
;

\ ---[ InitGL ]------------------------------------------------------
\ general OpenGL initialization function 

: InitGL ( -- boolean )
  \ Check on the ARB_multitexture extension availabilty
  InitMultiTexture to MultiTextureSupported
  \ Load in the texture
  LoadGLTextures 0= if
    FALSE                                        \ Return a bad value
  else
    GL_TEXTURE_2D gl-enable                  \ Enable texture mapping
    GL_SMOOTH gl-shade-model                  \ Enable smooth shading
    0e 0e 0e 0.5e gl-clear-color           \ Set the background black
    1e gl-clear-depth                            \ Depth buffer setup
    GL_DEPTH_TEST gl-enable                    \ Enable depth testing
    GL_LEQUAL gl-depth-func                \ type of depth test to do
    GL_PERSPECTIVE_CORRECTION_HINT GL_NICEST gl-hint    \ perspective
    InitLights                              \ Initialize the lighting
    TRUE                                        \ Return a good value
  then
;

\ ---[ VMatMulti ]---------------------------------------------------
\ Calculates v=vM, M Is 4x4 In Column-Major
\ v Is 4dim. Row (i.e. "Transposed")
\ For clarification, the data pointed to by the *m parameter is
\ stored in the 32-bit sfloat format, while the data pointed to by
\ the *v parameter is in gforth-normal 64-bit floating point values.

: VMatMult { *m *v -- }
  *m  0 sfarray-ndx SF@ *v 0 farray-ndx F@ F*
  *m  1 sfarray-ndx SF@ *v 1 farray-ndx F@ F* F+
  *m  2 sfarray-ndx SF@ *v 2 farray-ndx F@ F* F+
  *m  3 sfarray-ndx SF@ *v 3 farray-ndx F@ F* F+    \ leave on fstack

  *m  4 sfarray-ndx SF@ *v 0 farray-ndx F@ F*
  *m  5 sfarray-ndx SF@ *v 1 farray-ndx F@ F* F+
  *m  6 sfarray-ndx SF@ *v 2 farray-ndx F@ F* F+
  *m  7 sfarray-ndx SF@ *v 3 farray-ndx F@ F* F+    \ leave on fstack

  *m  8 sfarray-ndx SF@ *v 0 farray-ndx F@ F*
  *m  9 sfarray-ndx SF@ *v 1 farray-ndx F@ F* F+
  *m 10 sfarray-ndx SF@ *v 2 farray-ndx F@ F* F+
  *m 11 sfarray-ndx SF@ *v 3 farray-ndx F@ F* F+    \ leave on fstack
  
  *v 2 farray-ndx F!
  *v 1 farray-ndx F!
  *v 0 farray-ndx F!
  *m 15 sfarray-ndx SF@ *v 3 farray-ndx F!    \ homogenous coordinate
;

\ ---[ SetupBumps ]--------------------------------------------------
\ Sets Up The Texture-Offsets. All parameters are pointer to floating
\ point arrays.

\ n : Normal On Surface. Must Be Of Length 1
\ c : Current Vertex On Surface
\ l : Lightposition
\ s : Direction Of s-Texture-Coordinate In Object Space
\ t : Direction Of t-Texture-Coordinate In Object Space
\ s & t must be normalized!

create v[] 0e F, 0e F, 0e F,  \ vertex from current position to light
fvariable lenq                                    \ used to normalize

: SetupBumps { *n *c *l *s *t -- }
  \ Calculate v from current vertex <c> to light pos and normalize
  3 0 do
    *l i farray-ndx F@ *c i farray-ndx F@ F- v[] i farray-ndx F!
  loop

  v[] 0 farray-ndx F@ FDUP F*
  v[] 1 farray-ndx F@ FDUP F* F+
  v[] 2 farray-ndx F@ FDUP F* F+ FSQRT lenq F!

  3 0 do
    v[] i farray-ndx F@ lenq F@ F/ v[] i farray-ndx F!
  loop
      
  \ Project v such that we get two values on each texture/coord axis
  
  *s 0 farray-ndx F@ v[] 0 farray-ndx F@ F*
  *s 1 farray-ndx F@ v[] 1 farray-ndx F@ F* F+
  *s 2 farray-ndx F@ v[] 2 farray-ndx F@ F* F+
  Max-Emboss F*
  *c 0 farray-ndx F!
  
  *t 0 farray-ndx F@ v[] 0 farray-ndx F@ F*
  *t 1 farray-ndx F@ v[] 1 farray-ndx F@ F* F+
  *t 2 farray-ndx F@ v[] 2 farray-ndx F@ F* F+
  Max-Emboss F*
  *c 1 farray-ndx F!
;

\ ---[ DoLogo ]------------------------------------------------------
\ Billboards two logos

: DoLogo ( -- )
  GL_ALWAYS gl-depth-func
  GL_SRC_ALPHA GL_ONE_MINUS_SRC_ALPHA gl-blend-func
  GL_BLEND gl-enable
  GL_LIGHTING gl-disable
  gl-load-identity
  GL_TEXTURE_2D glLogo @ gl-bind-texture
  GL_QUADS gl-begin
    0e 1e gl-tex-coord-2f 0.23e -0.4e  -1e gl-vertex-3f
    1e 1e gl-tex-coord-2f 0.53e -0.4e  -1e gl-vertex-3f
    1e 0e gl-tex-coord-2f 0.53e -0.25e -1e gl-vertex-3f
    0e 0e gl-tex-coord-2f 0.23e -0.25e -1e gl-vertex-3f
  gl-end

  UseMultiTexture if
    GL_TEXTURE_2D MultiLogo @ gl-bind-texture
    GL_QUADS gl-begin
      0e 0e gl-tex-coord-2f -0.53e -0.25e -1e gl-vertex-3f
      1e 0e gl-tex-coord-2f -0.33e -0.25e -1e gl-vertex-3f
      1e 1e gl-tex-coord-2f -0.33e -0.15e -1e gl-vertex-3f
      0e 1e gl-tex-coord-2f -0.53e -0.15e -1e gl-vertex-3f
    gl-end
  then
;

\ ---[ DoMesh1TexelUnits ]-------------------------------------------
\ function to do bump-mapping without multitexturing

\ c[] holds the current vertex
\ n[] normalized normal of current surface
\ s[] s-texture coordinate direction, normalized
\ t[] t-texture coordinate direction, normalized
\ l[] hold the lightpos to be transformed into object space
\ Minv[] hold the inverted modelview matrix (16 fp values)

create c[] 0e F, 0e F, 0e F, 1e F,
create n[] 0e F, 0E f, 0e F, 1e F,
create s[] 0e F, 0E f, 0e F, 1e F,
create t[] 0e F, 0E f, 0e F, 1e F,
create l[] 0e F, 0E f, 0e F, 0e F,
create Minv[] 16 sfloats allot          \ 16 element array of sfloats

: SetFaceMesh1 ( r1 r2 r3 r4 f5 r6 r7 r8 r9 hi lo -- )
  t[] 2 farray-ndx F! t[] 1 farray-ndx F! t[] 0 farray-ndx F!
  s[] 2 farray-ndx F! s[] 1 farray-ndx F! s[] 0 farray-ndx F!
  n[] 2 farray-ndx F! n[] 1 farray-ndx F! n[] 0 farray-ndx F!
  do
    i data-ndx .x F@ c[] 0 farray-ndx F!
    i data-ndx .y F@ c[] 1 farray-ndx F!
    i data-ndx .z F@ c[] 2 farray-ndx F!
    n[] c[] l[] s[] t[] SetupBumps
    i data-ndx .tx F@ c[] 0 farray-ndx F@ F+
    i data-ndx .ty F@ c[] 1 farray-ndx F@ F+ gl-tex-coord-2f
    i data-ndx >R R@ .x F@ R@ .y F@ R> .z F@ gl-vertex-3f
  loop
;

\ ---[ DoMesh2TexelUnits ]-------------------------------------------  

: DoMesh1TexelUnits ( -- boolean )
  GL_COLOR_BUFFER_BIT GL_DEPTH_BUFFER_BIT OR gl-clear
  \ Build inverse modelview matrix first.
  \ This substitutes one push/pop with one gl-load-identity.
  \ Simply build it by doing all transformations negated, and in
  \ reverse order.
  gl-load-identity
  yrot F@ FNEGATE 0e 1e 0e gl-rotate-f
  xrot F@ FNEGATE 1e 0e 0e gl-rotate-f
  0e 0e zdepth F@ FNEGATE gl-translate-f
  GL_MODELVIEW_MATRIX Minv[] gl-get-float-v
  gl-load-identity
  0e 0e zdepth F@ gl-translate-f
  xrot F@ 1e 0e 0e gl-rotate-f
  yrot F@ 0e 1e 0e gl-rotate-f
  
  \ Transform the lightpos into object coordinates
  
  LightPosition[] 0 farray-ndx F@ l[] 0 farray-ndx F!
  LightPosition[] 1 farray-ndx F@ l[] 1 farray-ndx F!
  LightPosition[] 2 farray-ndx F@ l[] 2 farray-ndx F!
  1e l[] 3 farray-ndx F!                      \ homogenous coordinate
  Minv[] l[] VMatMult
  
  \ First pass rendering a cube obly out of bump map
  GL_TEXTURE_2D filter bump-ndx @ gl-bind-texture
  GL_BLEND gl-disable
  GL_LIGHTING gl-disable
  DoCube
  
  \ Second pass rendering a cube with correct emboss bump mapping
  \ but with no colors
  
  GL_TEXTURE_2D filter invbump-ndx @ gl-bind-texture
  GL_ONE GL_ONE gl-blend-func
  GL_LEQUAL gl-depth-func
  GL_BLEND gl-enable
  
  GL_QUADS gl-begin
     0e  0e  1e  1e 0e  0e 0e 1e  0e  4  0 SetFaceMesh1  \ Front Face
     0e  0e -1e -1e 0e  0e 0e 1e  0e  8  4 SetFaceMesh1   \ Back Face
     0e  1e  0e  1e 0e  0e 0e 0e -1e 12  8 SetFaceMesh1    \ Top Face
     0e -1e  0e -1e 0e  0e 0e 0e -1e 16 12 SetFaceMesh1 \ Bottom Face
     1e  0e  0e  0e 0e -1e 0e 1e  0e 20 16 SetFaceMesh1  \ Right Face
    -1e  0e  0e  0e 0e  1e 0e 1e  0e 24 20 SetFaceMesh1   \ Left Face     
  gl-end
  
  \ Third pass finishes rendering cube complete with lighting

  emboss FALSE = if
    GL_TEXTURE_ENV GL_TEXTURE_ENV_MODE GL_MODULATE S>F gl-tex-env-f
    GL_TEXTURE_2D filter texture-ndx @ gl-bind-texture
    GL_DST_COLOR GL_SRC_COLOR gl-blend-func
    GL_LIGHTING gl-enable
    DoCube
  then
  
  xrot F@ xspeed F@ F+ 
  fdup 360e F> if 360e F- then
  fdup   0e F< if 360e F+ then
  xrot F!
  yrot F@ xspeed F@ F+
  fdup 360e F> if 360e F- then
  fdup   0e F< if 360e F+ then
  yrot F!

  \ Last pass - do the logos
  DoLogo

  TRUE
;

\ ---[ DoMesh2TexelUnits ]-------------------------------------------  
\ Same as doMesh1TexelUnits except in 2 passes using 2 texel units

: SetFaceMesh2 ( r1 r2 r3 r4 f5 r6 r7 r8 r9 hi lo -- )
  t[] 2 farray-ndx F! t[] 1 farray-ndx F! t[] 0 farray-ndx F!
  s[] 2 farray-ndx F! s[] 1 farray-ndx F! s[] 0 farray-ndx F!
  n[] 2 farray-ndx F! n[] 1 farray-ndx F! n[] 0 farray-ndx F!
  do
    i data-ndx .x F@ c[] 0 farray-ndx F!
    i data-ndx .y F@ c[] 1 farray-ndx F!
    i data-ndx .z F@ c[] 2 farray-ndx F!
    n[] c[] l[] s[] t[] SetupBumps
    GL_TEXTURE0_ARB i data-ndx dup .tx F@ .ty F@
    gl-multi-tex-coord-2f-ARB
    GL_TEXTURE1_ARB i data-ndx dup .tx F@ c[] 0 farray-ndx F@ F+
    .ty F@ c[] 1 farray-ndx F@ F+ gl-multi-tex-coord-2f-ARB
    i data-ndx >R R@ .x F@ R@ .y F@ R> .z F@ gl-vertex-3f
  loop
;

: DoMesh2TexelUnits ( -- boolean )
  GL_COLOR_BUFFER_BIT GL_DEPTH_BUFFER_BIT OR gl-clear
  \ Build inverse modelview matrix first.
  \ This substitutes one push/pop with one gl-load-identity.
  \ Simply build it by doing all transformations negated, and in
  \ reverse order.
  gl-load-identity
  yrot F@ FNEGATE 0e 1e 0e gl-rotate-f
  xrot F@ FNEGATE 1e 0e 0e gl-rotate-f
  0e 0e zdepth F@ FNEGATE gl-translate-f
  GL_MODELVIEW_MATRIX Minv[] gl-get-float-v
  gl-load-identity
  0e 0e zdepth F@ gl-translate-f
  xrot F@ 1e 0e 0e gl-rotate-f
  yrot F@ 0e 1e 0e gl-rotate-f
  
  \ Transform the lightpos into object coordinates
  
  LightPosition[] 0 farray-ndx F@ l[] 0 farray-ndx F!
  LightPosition[] 1 farray-ndx F@ l[] 1 farray-ndx F!
  LightPosition[] 2 farray-ndx F@ l[] 2 farray-ndx F!
  1e l[] 3 farray-ndx F!                      \ homogenous coordinate
  Minv[] l[] VMatMult
  
  \ First pass:
  \   No Blending
  \   No Lighting  
  \
  \ Set up the texture-combiner 0 to
  \   Use bump-texture
  \   Use not-offset texture-coordinates
  \   Texture-operation GL_REPLACE, resulting in texture being drawn
  \
  \ Set up the texture-combiner 1 to
  \
  \   Offset texture-coordinates
  \   Texture-operation GL_ADD which is the multitexture equivalent
  \   to ONE, ONE- blending.
  \
  \ This will render a cube consisting out of grey-scale erode map.

  \ Texture-Unit #0
  GL_TEXTURE0_ARB gl-active-texture-ARB
  GL_TEXTURE_2D gl-enable
  GL_TEXTURE_2D filter bump-ndx @ gl-bind-texture
  GL_TEXTURE_ENV GL_TEXTURE_ENV_MODE GL_COMBINE_EXT S>F gl-tex-env-f
  GL_TEXTURE_ENV GL_COMBINE_RGB_EXT GL_REPLACE S>F gl-tex-env-f
  
  \ Texture-Unit #1
  GL_TEXTURE1_ARB gl-active-texture-ARB
  GL_TEXTURE_2D gl-enable
  GL_TEXTURE_2D filter invbump-ndx @ gl-bind-texture
  GL_TEXTURE_ENV GL_TEXTURE_ENV_MODE GL_COMBINE_EXT S>F gl-tex-env-f
  GL_TEXTURE_ENV GL_COMBINE_RGB_EXT GL_ADD S>F gl-tex-env-f

  \ General switches
  
  GL_BLEND gl-disable
  GL_LIGHTING gl-disable
  
  GL_QUADS gl-begin
     0e  0e  1e  1e 0e  0e 0e 1e  0e  4  0 SetFaceMesh2  \ Front Face
     0e  0e -1e -1e 0e  0e 0e 1e  0e  8  4 SetFaceMesh2   \ Back Face
     0e  1e  0e  1e 0e  0e 0e 0e -1e 12  8 SetFaceMesh2    \ Top Face
     0e -1e  0e -1e 0e  0e 0e 0e -1e 16 12 SetFaceMesh2 \ Bottom Face
     1e  0e  0e  0e 0e -1e 0e 1e  0e 20 16 SetFaceMesh2  \ Right Face
    -1e  0e  0e  0e 0e  1e 0e 1e  0e 24 20 SetFaceMesh2   \ Left Face
  gl-end

  \ Second Pass 
  \
  \ Use the base-texture 
  \ Enable Lighting 
  \ No offset texturre-coordinates => reset GL_TEXTURE-matrix 
  \ Reset texture environment to GL_MODULATE in order to do
  \ OpenGLLighting (doesn?t work otherwise!) 
  \
  \ This will render our complete bump-mapped cube. 

  GL_TEXTURE1_ARB gl-active-texture-ARB
  GL_TEXTURE_2D gl-disable
  GL_TEXTURE0_ARB gl-active-texture-ARB
  emboss FALSE = if
    GL_TEXTURE_ENV GL_TEXTURE_ENV_MODE GL_MODULATE S>F gl-tex-env-f
    GL_TEXTURE_2D filter texture-ndx @ gl-bind-texture
    GL_DST_COLOR GL_SRC_COLOR gl-blend-func
    GL_BLEND gl-enable
    GL_LIGHTING gl-enable
    DoCube
  then

  \ Last Pass 
  \
  \  Update Geometry (esp. rotations) 
  \  Do The Logos 
  
  xrot F@ xspeed F@ F+ 
  fdup 360e F> if 360e F- then
  fdup   0e F< if 360e F+ then
  xrot F!
  yrot F@ xspeed F@ F+
  fdup 360e F> if 360e F- then
  fdup   0e F< if 360e F+ then
  yrot F!
  
  \ LAST PASS: Do The Logos!
  DoLogo

  TRUE
;

\ ---[ DoMeshNoBumps ]-----------------------------------------------
\ function to draw cube without bump mapping

: DoMeshNoBumps ( -- boolean )
  GL_COLOR_BUFFER_BIT GL_DEPTH_BUFFER_BIT OR gl-clear
  gl-load-identity
  0e 0e zdepth F@ gl-translate-f
  
  xrot F@ 1e 0e 0e gl-rotate-f
  yrot F@ 0e 1e 0e gl-rotate-f
  
  UseMultiTexture if
    GL_TEXTURE1_ARB gl-active-texture-ARB
    GL_TEXTURE_2D gl-disable
    GL_TEXTURE0_ARB gl-active-texture-ARB
  then

  GL_BLEND gl-disable
  GL_TEXTURE_2D filter texture-ndx @ gl-bind-texture
  GL_DST_COLOR GL_SRC_COLOR gl-blend-func
  GL_LIGHTING gl-enable
  DoCube

  xrot F@ xspeed F@ F+ 
  fdup 360e F> if 360e F- then
  fdup   0e F< if 360e F+ then
  xrot F!
  yrot F@ xspeed F@ F+
  fdup 360e F> if 360e F- then
  fdup   0e F< if 360e F+ then
  yrot F!

  \ LAST PASS: Do The Logos!
  DoLogo

  TRUE
;

\ ---[ ShutDown ]----------------------------------------------------
\ Close down the system gracefully ;-)

: ShutDown ( -- )
  FALSE to opengl-exit-flag           \ reset this flag for next time
  NumTextures texture gl-delete-textures          \ clean up textures
  NumBumps    bump    gl-delete-textures
  NumInvBumps invbump gl-delete-textures
  sdl-quit                               \ close down the SDL systems
;

fvariable fps-seconds
fvariable fps-count
0 value   fps-ticks
0 value   fps-t0
0 value   fps-frames
0 value   fps-line

\ ---[ Reset-FPS-Counter ]-------------------------------------------

: Reset-FPS-Counter ( -- )
  sdl-get-ticks to fps-t0
  0 to fps-frames
;

\ ---[ Display-FPS ]-------------------------------------------------

: Display-FPS ( -- )
  sdl-get-ticks to fps-ticks
  fps-ticks fps-t0 - 1000 >= if
    fps-ticks fps-t0 - S>F 1000e F/ fps-seconds F!
    fps-frames S>F fps-seconds F@ F/ fps-count F!
    0 fps-line at-xy 50 spaces           \ clear previous fps display
    0 fps-line at-xy                          \ display new fps count
    fps-frames . ." frames in " 
    fps-seconds F@ F>S . ." seconds = " 
    fps-count F@ F>S . ." FPS" cr
    fps-ticks to fps-t0
    0 to fps-frames
  then
;

\ ---[ DrawGLScene ]-------------------------------------------------
\ Here goes our drawing code 

: DrawGLScene ( -- boolean )
  bumps if
    UseMultiTexture MaxTexelUnits @ 1 > AND if
      DoMesh2TexelUnits FALSE = if
        FALSE
        EXIT
      then
    else
      DoMesh1TexelUnits FALSE = if
        FALSE
        EXIT
      then
    then
  else
    DoMeshNoBumps FALSE = if
      FALSE
      EXIT
    then
  then

  sdl-gl-swap-buffers                         \ Draw it to the screen

  fps-frames 1+ to fps-frames   \ Gather  our frames per second count
  Display-FPS          \ Display the FPS count to the terminal window

  xrot F@ 0.3e F+ xrot F!                      \ increment x rotation
  yrot F@ 0.2e F+ yrot F!                      \ increment y rotation
  zrot F@ 0.4e F+ zrot F!                      \ increment z rotation
  
  TRUE                                          \ Return a good value
;

