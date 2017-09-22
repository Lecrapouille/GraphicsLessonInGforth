\ ===[ Code Addendum 03 ]============================================
\                 gforth: OpenGL Graphics Lesson 01
\ ===================================================================
\           File: mini-opengl-1.01.fs
\         Author: Timothy Trussell
\           Date: 07/06/2010
\    Description: OpenGL libcc interface for NeHe OpenGL Tutorial 01
\   Forth System: gforth-0.7.0
\      Assembler: Built-in FORTH assembler
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-23
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\ This libcc interface contains the cumulatively added OpenGL code
\ function calls that are required for use with the gforth version of
\ the NeHe OpenGL Tutorial Lessons.
\ ===================================================================

c-library mini_opengl_lib

s" GL" add-lib
s" GLU" add-lib

\c #if defined(__APPLE__) && defined(__MACH__)
\c      #include <OpenGL/gl.h>
\c      #include <OpenGL/glu.h>
\c #else
\c      #include <GL/gl.h>
\c      #include <GL/glu.h>
\c #endif

\ Initial Entries for Lesson 01
c-function gl-clear            glClear                      n -- void
c-function gl-clear-color      glClearColor           r r r r -- void
c-function gl-clear-depth      glClearDepth                 r -- void
c-function gl-enable           glEnable                     n -- void
c-function gl-depth-func       glDepthFunc                  n -- void
c-function gl-hint             glHint                     n n -- void
c-function gl-load-identity    glLoadIdentity                 -- void
c-function gl-matrix-mode      glMatrixMode                 n -- void
c-function gl-shade-model      glShadeModel                 n -- void
c-function gl-viewport         glViewport             n n n n -- void
c-function glu-perspective     gluPerspective         r r r r -- void

end-c-library

include glconstants.fs

