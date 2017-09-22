\ ===================================================================
\        Program: mini-mixer-1.00.fs
\         Author: Timothy Trussell
\           Date: 08/01/2010
\    Description: SDL_Mixer libcc interface
\   Forth System: gforth-0.7.0
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-24
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\ This libcc interface contains the SDL_Mixer code required for use
\ with the NeHe OpenGL Tutorials.
\ ===================================================================
\ This file should be loaded *after* the mini-sdl-1.xx.fs file, as
\ it requires the definitions in the sdlconstants.fs file.
\ ===================================================================

UseLibUtil [if]
  c-library mini_mixer_lib
[else]
  c-library mini_mixer_lib21
[then]

s" SDL_mixer" add-lib

\c #include <SDL/SDL_mixer.h>

\ Initial Entries for Lesson 21
c-function mix-close-audio      Mix_CloseAudio                -- void
c-function mix-free-chunk       Mix_FreeChunk               a -- void
c-function mix-free-music       Mix_FreeMusic               a -- void
c-function mix-halt-channel     Mix_HaltChannel                n -- n
c-function mix-halt-music       Mix_HaltMusic                    -- n
c-function mix-load-mus         Mix_LoadMUS                    a -- a
c-function mix-load-wav         Mix_LoadWAV                    a -- a
c-function mix-open-audio       Mix_OpenAudio            n n n n -- n
c-function mix-play-music       Mix_PlayMusic                a n -- n
c-function mix-play-channel     Mix_PlayChannel            n a n -- n

end-c-library

\ From SDL_audio.h
AUDIO_U16LSB constant AUDIO_U16
AUDIO_S16LSB constant AUDIO_S16

\ @name Native audio byte ordering

SDL_BYTEORDER SDL_LIL_ENDIAN = [if]        \ requires sdlconstants.fs
  AUDIO_U16LSB constant AUDIO_U16SYS
  AUDIO_S16LSB constant AUDIO_S16SYS
[else]
  AUDIO_U16MSB constant AUDIO_U16SYS
  AUDIO_S16MSB constant AUDIO_S16SYS
[then]

