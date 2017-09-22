\ ===================================================================
\           File: opengllib-1.24.fs
\         Author: Jeff Molofee
\  Linux Version: DarkAlloy
\ gForth Version: Timothy Trussell, 05/01/2011
\    Description: Tokens, extensions, scissor testing, TGA
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.32-31
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\                       NeHe Productions 
\                    http://nehe.gamedev.net/
\ ===================================================================
\                   OpenGL Tutorial Lesson 21
\ ===================================================================
\ This code was created by Jeff Molofee '99 
\ (ported to Linux/SDL by Dark Alloy)
\ Visit Jeff at http://nehe.gamedev.net/
\ ===================================================================

\ ---[ UseLibUtil ]--------------------------------------------------
\ Conditional compilation of the libcc interfaces:
\ -- Set to 1 if you use the LibUtil script to copy the files to
\    the ~/.gforth directory.
\ -- Set to 0 to use the files from the Lesson directory (PWD).

0 constant UseLibUtil

UseLibUtil [if]
  require ~/.gforth/opengl-libs/mini-opengl-current.fs
  require ~/.gforth/opengl-libs/mini-sdl-current.fs
  require ~/.gforth/opengl-libs/sdlkeysym.fs
[else]
  require mini-opengl-1.24.fs
  require mini-sdl-1.02.fs
  require sdlkeysym.fs
[then]

\ ---[ Prototype Listing ]-------------------------------------------
\ : s-itos                      ( n -- str len )
\ : s-zlen                      { *str -- len }
\ : s-token                     { *src _c -- *str len }
\ : Generate-Texture            { *src -- }
\ : load-image                  ( str len ndx -- boolean )
\ : Flip-Characters             ( *src -- )
\ : LoadGLTextures              ( -- boolean )
\ : BuildFont                   ( -- )
\ : glPrint                     { _x _y *str _len _set -- }  
\ : HandleKeyPress              ( &event -- )
\ : HandleKeyRelease            ( &event -- )
\ : InitGL                      ( -- boolean )
\ : ShutDown                    ( -- )
\ : Reset-FPS-Counter           ( -- )
\ : Display-FPS                 ( -- )
\ : DrawGLScene                 ( -- boolean )
\ ------------------------------------------------[End Prototypes]---

\ ---[ Variable Declarations ]---------------------------------------

1 CONSTANT NumTextures                    \ number of textures to use
1 CONSTANT NumImages                  \ number of images being loaded

0 VALUE scroll                        \ Used For Scrolling The Screen
0 VALUE maxtokens                    \ Number Of Extensions Supported
0 VALUE swidth                                        \ Scissor Width
0 VALUE sheight                                      \ Scissor Height
0 VALUE scroller               \ Used to control the Scrolling action

0 VALUE ExtensionsLoaded                        \ only load them once
0 VALUE *Extensions           \ where we loaded the extensions string

variable baselist                \ base display list for the font set

\ Allot space for texture pointers and init the memory

create texture    here NumTextures CELLS dup allot 0 fill    \ OpenGL
create teximage[] here NumTextures CELLS dup allot 0 fill       \ SDL

0 baselist !

\ ---[ Array Index Functions ]---------------------------------------
\ Index functions to access the arrays

: texture-ndx ( n -- *texture[n] ) NumTextures MOD CELLS texture + ;
: teximage-ndx ( n -- *TextureImage[n] ) CELLS teximage[] + ;

\ ---[ s-itos ]------------------------------------------------------
\ Converts an integer to a string

: s-itos   ( n -- str len ) 0 <# #S #> ;

\ ---[ s-zlen ]------------------------------------------------------
\ Returns the length of a NULL terminated string

: s-zlen { *str -- len }
  0 begin *str over + C@ 0= if 1 else 1+ 0 then until
;

\ ---[ s-token ]-----------------------------------------------------
\ Returns the *str/len pair from the current address to the next
\ address of the specified token character - or end of list (NULL)
\ After each pass, the (token) pointer is set to the last occurrence
\ of the token found.

0 VALUE (token)                            \ addr of last token found

: s-token { *src _c -- *str len }
  *src 0= if (token) 1+ to *src then   \ if NULL passed use last addr
  0                               \ initial count value for this pass
  begin *src over + C@ dup _c = swap 0= OR if 1 else 1+ 0 then until
  *src over + to (token)                          \ set for next time
  *src swap
;

\ ---[ Texture Generation Functions ]--------------------------------

: Generate-Texture { *src -- }
  GL_TEXTURE_2D 0
  *src sdl-surface-format @ sdl-pixelformat-bitsperpixel C@ 32 = if
    GL_RGBA                                         \ for 32bpp image
  else
    GL_RGB                                          \ for 24bpp image
  then               \ specifies # of color components in the texture
  *src sdl-surface-w @                       \ width of texture image
  *src sdl-surface-h @                      \ height of texture image
  0
  3 pick                                       \ format of the pixels
  GL_UNSIGNED_BYTE
  *src sdl-surface-pixels @                 \ address of texture data
  gl-tex-image-2d                               \ finally generate it
;

\ ---[ load-image ]--------------------------------------------------
\ Attempt to load the texture images into SDL surfaces, saving the
\ result into the teximage[] array; Return TRUE if result from
\ sdl-img-load is <> 0; else return FALSE

: load-image ( str len ndx -- boolean )
  >R zstring sdl-img-load dup R> teximage-ndx ! 0<>
;

\ ---[ Flip-Characters ]---------------------------------------------
\ Reverses the character line order in the image prior to processing
\ by the OpenGL texture generation procedures.

: Flip-Characters ( *src -- )
  0 0 0 { *src *tdata *timage _b/l -- }                      \ locals
  *src sdl-surface-pixels @ to *tdata         \ pointer to image data
  here 256 mod 256 swap - here + to *timage      \ set buffer address
  *src sdl-surface-pitch sw@ to _b/l          \ bytes/line of surface
  *tdata *timage _b/l *src sdl-surface-h @ * cmove
  16 0 do                             \ # of character rows to modify
    16 0 do                                \ character line to modify
      *timage j _b/l * 16 * + 15 i - _b/l * +        \ source pointer
      *tdata  j _b/l * 16 * + i _b/l * +               \ dest pointer
      _b/l cmove
    loop
  loop
;

\ ---[ LoadGLTextures ]----------------------------------------------
\ Loads the image file(s) and generates OpenGL textures from them

: LoadGLTextures ( -- status )
  FALSE { _status -- status }                                \ locals
  teximage[] NumTextures CELLS 0 fill       \ erase data pointers
\  s" data/font.tga0" 0 load-image if
  s" data/font.bmp0" 0 load-image if
    0 teximage-ndx @ Flip-Characters   \ reverse character line order
    TRUE to _status                                \ set return value
    NumTextures texture gl-gen-textures         \ create the textures
    GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture      \ load texture
    0 teximage-ndx @ Generate-Texture              \ Generate texture
    GL_TEXTURE_2D GL_TEXTURE_MIN_FILTER GL_LINEAR gl-tex-parameter-i
    GL_TEXTURE_2D GL_TEXTURE_MAG_FILTER GL_LINEAR gl-tex-parameter-i
  else
    \ At least one of the images did not load, so exit
    cr ." Error: texture image could not be loaded!" cr
  then
  \ Free the image surface that was created
  0 teximage-ndx @ dup 0<> if sdl-freesurface else drop then
  _status                      \ exit with return value: 0=fail;-1=ok
;

\ ---[ BuildFont ]---------------------------------------------------
\ Function to build our OpenGL font display list

fvariable bf-cx                         \ holds our x character coord
fvariable bf-cy                         \ holds our y character coord

: BuildFont ( -- )
  \ Create 256 display lists
  256 gl-gen-lists baselist !
  \ Select our font texture
  GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture
  \ Loop thru all 256 lists
  256 0 do  
    \ X Position of current character
    1e i 16 MOD S>F 16e F/ F- bf-cx F!
    \ Y Position of current character
    1e i 16 /   S>F 16e F/ F- bf-cy F!
    \ Start building a list
    baselist @ 255 i - + GL_COMPILE gl-new-list
      \ use a quad for each character
      GL_QUADS gl-begin
        \ texture coordinate - bottom left
        bf-cx F@ 0.0625e F- bf-cy F@ gl-tex-coord-2f
        \ vertex coordinate - bottom left
        0 0 gl-vertex-2i
        \ texture coordinate - bottom right
        bf-cx F@ bf-cy F@ gl-tex-coord-2f
        \ vertex coordinate - bottom right
        16 0 gl-vertex-2i
        \ texture coordinate - top right
        bf-cx F@ bf-cy F@ 0.0625e F- gl-tex-coord-2f
        \ vertex coordinate - top right
        16 16 gl-vertex-2i
        \ texture coordinate - top left
        bf-cx F@ 0.0625e F- bf-cy F@ 0.0625e F- gl-tex-coord-2f
        \ vertex coordinate - top left
        0 16 gl-vertex-2i
      gl-end
      \ Move to the right of the character
      14e 0e 0e gl-translate-d
    gl-end-list
  loop
;

\ ---[ glPrint ]-----------------------------------------------------
\ Prints a string
\ <set> selects Normal <0>, or Italic <1> from the font.bmp image.

: glPrint { _x _y *str _len _set -- }  
  _set 1 > if 1 to _set then
  GL_TEXTURE_2D gl-enable                    \ Enable texture mapping
  GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture  \ Select our texture
  gl-load-identity                       \ Reset the modelview matrix
  _x S>F _y S>F 0e0 gl-translate-d \ Position text (0,0==bottom left)
  baselist @ 32 - 128 _set * + gl-list-base     \ Choose the font set
  1e0 2e0 1e0 gl-scale-f             \ scale width and height of font
  _len GL_UNSIGNED_BYTE *str gl-call-lists           \ Write the text
  GL_TEXTURE_2D gl-disable                  \ Disable texture mapping
;

\ ---[ Keyboard Flags ]----------------------------------------------
\ Flags needed to prevent constant toggling if the keys that they
\ represent are held down during program operation.
\ By checking to see if the specific flag is already set, we can then
\ choose to ignore the current keypress event for that key.

0 value key-ESC
0 value key-F1
0 value key-Up
0 value key-Dn

\ ---[ HandleKeyPress ]----------------------------------------------
\ function to handle key press events:
\   ESC      exits the program
\   F1       toggles between fullscreen and windowed modes
\   Up       scrolls extension list up
\   Down     scrolls extension list down

: HandleKeyPress ( &event -- )
  sdl-keyboard-event-keysym sdl-keysym-sym uw@
  case
    SDLK_ESCAPE of TRUE to opengl-exit-flag endof
    SDLK_F1     of key-F1 FALSE = if        \ skip if being held down
                     screen sdl-wm-togglefullscreen drop
                     TRUE to key-F1            \ set key pressed flag
                   then
                endof
    SDLK_UP     of key-Up FALSE = if        \ skip if being held down
                     -1 to scroller 
                     TRUE to key-Up            \ set key pressed flag
                   then
                endof
    SDLK_DOWN   of key-Dn FALSE = if        \ skip if being held down
                     1 to scroller 
                     TRUE to key-Dn            \ set key pressed flag
                   then
                endof
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
    SDLK_UP       of FALSE to key-Up 0 to scroller endof
    SDLK_DOWN     of FALSE to key-Dn 0 to scroller endof
  endcase
;

\ ---[ InitGL ]------------------------------------------------------
\ general OpenGL initialization function 

: InitGL ( -- boolean )
  \ Load in the texture
  LoadGLTextures 0= if
    FALSE                                        \ Return a bad value
  else
    BuildFont                                        \ build the font
    GL_SMOOTH gl-shade-model                  \ Enable smooth shading
    0e 0e 0e 0.5e gl-clear-color           \ Set the background black
    1e gl-clear-depth                            \ Depth buffer setup
    GL_TEXTURE_2D 0 texture-ndx @ gl-bind-texture    \ Select texture
    TRUE                                        \ Return a good value
  then
;

\ ---[ ShutDown ]----------------------------------------------------
\ Close down the system gracefully ;-)

: ShutDown ( -- )
  FALSE to opengl-exit-flag           \ reset this flag for next time
  baselist @ 256 gl-delete-lists             \ clean up the font list
  NumTextures texture gl-delete-textures          \ clean up textures
  FALSE to opengl-exit-flag          \ reset these flag for next time
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
  0 0 0 { _cnt *next _len -- boolean }
  \ Clear the screen and the depth buffer 
  GL_COLOR_BUFFER_BIT GL_DEPTH_BUFFER_BIT OR gl-clear

  gl-load-identity                                   \ restore matrix

  1e 0.5e 0.5e gl-color-3f                  \ set color to bright red
  50 16 s" Renderer" 1 glPrint                     \ display Renderer
  80 48 s" Vendor"   1 glPrint                  \ display Vendor Name
  66 80 s" Version"  1 glPrint                      \ display Version

  1e 0.7e 0.4e gl-color-3f                      \ set color to orange

  200 16 GL_RENDERER gl-get-string dup s-zlen 1 glPrint
  200 48 GL_VENDOR   gl-get-string dup s-zlen 1 glPrint
  200 80 GL_VERSION  gl-get-string dup s-zlen 1 glPrint

  0.5e 0.5e 1e gl-color-3f                 \ set color to bright blue
  192 432 s" NeHe Productions" 1 glPrint        \ at bottom of screen

  gl-load-identity                       \ reset the modelview matrix
  1e 1e 1e gl-color-3f                           \ set color to white
  GL_LINE_STRIP gl-begin                  \ start drawing line strips
    639e 417e gl-vertex-2d                  \ top/right of bottom box
    0e 417e gl-vertex-2d                     \ top/left of bottom box
    0e 480e gl-vertex-2d                   \ lower/left of bottom box
    639e 480e gl-vertex-2d                \ lower/right of bottom box
    639e 128e gl-vertex-2d            \ up to bottom/right of top box
  gl-end
  GL_LINE_STRIP gl-begin           \ start drawing another line strip
    0e 128e gl-vertex-2d                     \ bottom/left of top box
    639e 128e gl-vertex-2d                  \ bottom/right of top box
    639e 1e gl-vertex-2d                       \ top/right of top box
    0e 1e gl-vertex-2d                          \ top/left of top box
    0e 417e gl-vertex-2d             \ down to top/left of bottom box
  gl-end

  \ Set up the viewport for displaying the Extensions listing

  1                             \ x
  0.135416e sheight S>F F* F>S  \ x y
  swidth 2 -                    \ x y width
  0.597916e sheight S>F F* F>S  \ x y width height
  gl-scissor                    \ --          \ define scissor region
  GL_SCISSOR_TEST gl-enable                  \ enable scissor testing

  \ Load the GL Extensions text into a buffer now
  \ A flag will be set after this has been executed once, so that the
  \ next run of the code will not re-load the Extensions data.

  ExtensionsLoaded 0= if                  \ load them if this flag==0
    GL_EXTENSIONS gl-get-string >R       \ get Extension text address
    here dup to *Extensions R@ s-zlen 1+ dup allot 0 fill \ allot buf
    R@ *Extensions R> s-zlen cmove              \ copy data to buffer
    1 to ExtensionsLoaded         \ toggle flag that buffer is loaded
  then

  *Extensions $20 s-token        \ Get the first entry in the listing
  begin
    (token) C@ 0<>        \ loop thru the entire string until 0 found
  while
    to _len to *next
    _cnt 1+ to _cnt
    _cnt maxtokens > if _cnt to maxtokens then
    0.5e 1e 0.5e gl-color-3f              \ set color to bright green
    0 96 _cnt 32 * + scroll - _cnt s-itos 0 glPrint     \ Extension #
    1e 1e 0.5e gl-color-3f                      \ set color to yellow
    50 96 _cnt 32 * + scroll - *next _len 0 glPrint  \ Extension text
    NULL $20 s-token                          \ search for next token
  repeat
  2DROP

  GL_SCISSOR_TEST gl-disable                \ disable scissor testing  

  sdl-gl-swap-buffers                         \ Draw it to the screen

  fps-frames 1+ to fps-frames   \ Gather  our frames per second count
  Display-FPS          \ Display the FPS count to the terminal window

  TRUE                                          \ Return a good value
;
