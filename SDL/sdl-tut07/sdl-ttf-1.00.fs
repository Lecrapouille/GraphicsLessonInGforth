\ ===[ Code Addendum 03 ]============================================
\             gforth: SDL/OpenGL Graphics Part VII
\ ===================================================================
\    File Name: sdl-ttf-1.00.fs
\      Version: 1.00
\       Author: Timothy Trussell
\         Date: 03/24/2010
\  Description: SDL TrueType Font Interface for gforth
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-20
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\                 SDL_ttf v1.00 Interface for gforth
\ ===================================================================

\ ---[ Change Log ]--------------------------------------------------
\
\ 03/24/2010    Coded SDL_ttf extension library functions.   rev 1.00
\               Had to add a C wrapper to get SDL_Color
\               parameters to pass correctly.
\
\ -----------------------------------------------[End Change Log ]---

\ ---[ gforth Prototypes ]-------------------------------------------

\ ttf-init                      ( -- f )
\ ttf-quit                      ( -- )
\ ttf-openfont                  ( *str len ptsize -- *ttf )
\ ttf-closefont                 ( *ttf -- )
\ ttf-ttf-rendertext-solid      ( *font *text *fg -- *dst )
\ ttf-ttf-rendertext-shaded     ( *font *text *fg *bg -- *dst )
\
\ ------------------------------------------------[End Prototypes]---

c-library ttf_lib
s" SDL_ttf" add-lib

\c #include <SDL/SDL.h>
\c #include <SDL/SDL_ttf.h>

\c // SDL_ttf library functions

c-function ttf-init             TTF_Init                -- n
c-function ttf-quit             TTF_Quit                -- void
c-function ttf-openfont         TTF_OpenFont            a n -- a
c-function ttf-closefont        TTF_CloseFont           a -- void

\c SDL_Surface * My_TTF_RT_Solid(TTF_Font *myfont,\
\c                       const char *mytext,\
\c                       SDL_Color *myfg)
\c { SDL_Color localfg;
\c   localfg.r = myfg->r;
\c   localfg.g = myfg->g;
\c   localfg.b = myfg->b;
\c   localfg.unused = 0;
\c   return TTF_RenderText_Solid(myfont,mytext,localfg);
\c }

\c SDL_Surface * My_TTF_RT_Shaded(TTF_Font *myfont,\
\c                       const char *mytext,\
\c                       SDL_Color *myfg,\
\c                       SDL_Color *mybg)
\c { SDL_Color localfg;
\c   SDL_Color localbg;
\c   localfg.r = myfg->r;  localbg.r = mybg->r;
\c   localfg.g = myfg->g;  localbg.g = mybg->g;
\c   localfg.b = myfg->b;  localbg.b = mybg->b;
\c   localfg.unused = 0;   localbg.unused = 0;
\c   return TTF_RenderText_Shaded(myfont,mytext,localfg,localbg);
\c }

c-function ttf-rendertext-solid  My_TTF_RT_Solid        a a a -- a
c-function ttf-rendertext-shaded My_TTF_RT_Shaded       a a a a -- a
    
end-c-library

\ ---[ TTF Constants ]-----------------------------------------------

%0000 constant TTF_STYLE_NORMAL
%0001 constant TTF_STYLE_BOLD
%0010 constant TTF_STYLE_ITALIC
%0100 constant TTF_STYLE_UNDERLINE

%sdl-quiet 0= [if]
.( TTF Interface loaded successfully.) cr
[then]

