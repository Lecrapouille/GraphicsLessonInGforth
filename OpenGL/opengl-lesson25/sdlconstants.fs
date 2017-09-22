\ ===================================================================
\    File Name: sdlconstants.fs
\  Description: SDL constants definitions
\ Forth System: gforth-0.7.0
\ ===================================================================

[IFUNDEF] =:  : =:  CONSTANT ;  [ENDIF]

1       =: SDL_MAJOR_VERSION
2       =: SDL_MINOR_VERSION
14      =: SDL_PATCHLEVEL                        \ may not be correct

\ Initialization Flags for sdl-init

$00000001       =: SDL_INIT_TIMER
$00000010       =: SDL_INIT_AUDIO
$00000020       =: SDL_INIT_VIDEO
$00000100       =: SDL_INIT_CDROM
$00000200       =: SDL_INIT_JOYSTICK
$00100000       =: SDL_INIT_NOPARACHUTE
$01000000       =: SDL_INIT_EVENTTHREAD
$0000FFFF       =: SDL_INIT_EVERYTHING

$00000000       =: SDL_SWSURFACE
$00000001       =: SDL_HWSURFACE
$00000004       =: SDL_ASYNCBLIT
$10000000       =: SDL_ANYFORMAT
$20000000       =: SDL_HWPALETTE
$40000000       =: SDL_DOUBLEBUF
$80000000       =: SDL_FULLSCREEN
$00000002       =: SDL_OPENGL
$0000000        =: SDL_OPENGLBLIT
$00000010       =: SDL_RESIZABLE
$00000020       =: SDL_NOFRAME
$00000100       =: SDL_HWACCEL
$00001000       =: SDL_SRCCOLORKEY
$00002000       =: SDL_RLEACCELOK
$00004000       =: SDL_RLEACCEL
$00010000       =: SDL_SRCALPHA
$01000000       =: SDL_PREALLOC
$32315659       =: SDL_YV12_OVERLAY
$56555949       =: SDL_IYUV_OVERLAY
$32595559       =: SDL_YUY2_OVERLAY
$59565955       =: SDL_UYVY_OVERLAY
$55595659       =: SDL_YVYU_OVERLAY

\ Big/Little Endian specific
\ Set SDL_BYTEORDER for the Endian-ness of your computer.
\ My Athlon 64 X2 is Little Endian.

1234    =: SDL_BYTEORDER
1234    =: SDL_LIL_ENDIAN
4321    =: SDL_BIG_ENDIAN

0       =: NULL
1       =: SDL_VIDEO_OPENGL

\ ---[ SDL_Video ]---------------------------------------------------

1       =: SDL_VIDEO_DRIVER_DC
1       =: SDL_VIDEO_DRIVER_CYBERGRAPHICS
1       =: SDL_VIDEO_DRIVER_DUMMY
1       =: SDL_VIDEO_DRIVER_AALIB
1       =: SDL_VIDEO_DRIVER_CACA
1       =: SDL_VIDEO_DRIVER_DGA
1       =: SDL_VIDEO_DRIVER_DIRECTFB
1       =: SDL_VIDEO_DRIVER_FBCON
1       =: SDL_VIDEO_DRIVER_SVGALIB
1       =: SDL_VIDEO_DRIVER_X11
1       =: SDL_VIDEO_DRIVER_X11_DGAMOUSE
1       =: SDL_VIDEO_DRIVER_X11_DPMS
1       =: SDL_VIDEO_DRIVER_X11_VIDMODE
1       =: SDL_VIDEO_DRIVER_X11_XINERAMA
1       =: SDL_VIDEO_DRIVER_X11_XME
1       =: SDL_VIDEO_DRIVER_X11_XRANDR
1       =: SDL_VIDEO_DRIVER_X11_XV
1       =: SDL_VIDEO_DRIVER_DRAWSPROCKET
1       =: SDL_VIDEO_DRIVER_TOOLBOX
1       =: SDL_VIDEO_DRIVER_QUARTZ
1       =: SDL_VIDEO_OPENGL_GLX
1       =: SDL_VIDEO_DRIVER_OS2FS
1       =: SDL_VIDEO_DRIVER_GAPI
1       =: SDL_VIDEO_DRIVER_DDRAW
1       =: SDL_VIDEO_DRIVER_WINDIB
1       =: SDL_VIDEO_OPENGL_WGL

255     =: SDL_ALPHA_OPAQUE
0       =: SDL_ALPHA_TRANSPARENT
$01     =: SDL_LOGPAL
$02     =: SDL_PHYSPAL

\ ---[ SDL_Audio ]---------------------------------------------------

$0008   =: AUDIO_U8
$8008   =: AUDIO_S8
$0010   =: AUDIO_U16LSB
$8010   =: AUDIO_S16LSB
$1010   =: AUDIO_U16MSB
$9010   =: AUDIO_S16MSB
128     =: SDL_MIX_MAXVOLUME
$00     =: SDL_AUDIO_TRACK
1       =: SDL_AUDIO_DRIVER_DC
1       =: SDL_AUDIO_DRIVER_AHI
1       =: SDL_AUDIO_DRIVER_DISK
1       =: SDL_AUDIO_DRIVER_DUMMY
1       =: SDL_AUDIO_DRIVER_ALSA
1       =: SDL_AUDIO_DRIVER_ARTS
1       =: SDL_AUDIO_DRIVER_ESD
1       =: SDL_AUDIO_DRIVER_NAS
1       =: SDL_AUDIO_DRIVER_OSS
1       =: SDL_AUDIO_DRIVER_SNDMGR
1       =: SDL_AUDIO_DRIVER_COREAUDIO
1       =: SDL_AUDIO_DRIVER_DSOUND
1       =: SDL_AUDIO_DRIVER_WAVEOUT
1       =: SDL_AUDIO_DRIVER_DART

\ ---[ SDL_Timer ]---------------------------------------------------

1       =: SDL_TIMER_AMIGA
1       =: SDL_TIMER_DC
1       =: SDL_TIMER_UNIX
1       =: SDL_TIMER_MACOS
1       =: SDL_TIMERS_DISABLED
1       =: SDL_TIMER_OS2
1       =: SDL_TIMER_WINCE
1       =: SDL_TIMER_WIN32
10      =: SDL_TIMESLICE
10      =: TIMER_RESOLUTION


\ ---[ SDL_Joystick and Mouse ]--------------------------------------

1       =: SDL_JOYSTICK_DUMMY
1       =: SDL_JOYSTICK_MACOS
1       =: SDL_JOYSTICK_IOKIT
1       =: SDL_JOYSTICK_DISABLED
1       =: SDL_JOYSTICK_WINMM
1       =: SDL_JOYSTICK_OS2
1       =: SDL_JOYSTICK_AMIGA
1       =: SDL_JOYSTICK_DC
1       =: SDL_JOYSTICK_LINUX
$00     =: SDL_HAT_CENTERED
$01     =: SDL_HAT_UP
$02     =: SDL_HAT_RIGHT
$04     =: SDL_HAT_DOWN
$08     =: SDL_HAT_LEFT
-1      =: SDL_ALL_HOTKEYS
500     =: SDL_DEFAULT_REPEAT_DELAY
30      =: SDL_DEFAULT_REPEAT_INTERVAL
1       =: SDL_BUTTON_LEFT
2       =: SDL_BUTTON_MIDDLE
3       =: SDL_BUTTON_RIGHT
4       =: SDL_BUTTON_WHEELUP
5       =: SDL_BUTTON_WHEELDOWN

\ ---[ OS types ]----------------------------------------------------

1       =: __AIX__
1       =: __AMIGA__
1       =: __BEOS__
1       =: __BSDI__
1       =: __DREAMCAST__
1       =: __FREEBSD__
1       =: __HPUX__
1       =: __IRIX__
1       =: __LINUX__
1       =: __MACOSX__
1       =: __MACOS__
1       =: __NETBSD__
1       =: __OPENBSD__
1       =: __OS2__
1       =: __OSF__
1       =: __QNXNTO__
1       =: __RISCOS__
1       =: __SOLARIS__
1       =: __WIN32__
1       =: WIN32_LEAN_AND_MEAN

\ ---[ CDROM ]-------------------------------------------------------

1       =: SDL_CDROM_DUMMY
1       =: SDL_CDROM_DC
1       =: SDL_CDROM_LINUX
1       =: SDL_CDROM_MACOS
1       =: SDL_CDROM_MACOSX
1       =: SDL_CDROM_DISABLED
1       =: SDL_CDROM_OS2
1       =: SDL_CDROM_WIN32
75      =: CD_FPS

\ ---[ Threads ]-----------------------------------------------------

1       =: SDL_THREAD_AMIGA
1       =: SDL_THREAD_DC
1       =: SDL_THREAD_PTHREAD
1       =: SDL_THREAD_PTHREAD_RECURSIVE_MUTEX
1       =: SDL_THREADS_DISABLED
1       =: SDL_THREAD_OS2
1       =: SDL_THREAD_WIN32

\ ---[ LoadSO ]------------------------------------------------------

1       =: SDL_LOADSO_DUMMY
1       =: SDL_LOADSO_DLOPEN
1       =: SDL_LOADSO_DISABLED
1       =: SDL_LOADSO_MACOS
1       =: SDL_LOADSO_DLCOMPAT
1       =: SDL_LOADSO_OS2
1       =: SDL_LOADSO_WIN32

\ ---[ SDL_GLattr ]--------------------------------------------------

0  =: SDL_GL_RED_SIZE
1  =: SDL_GL_GREEN_SIZE
2  =: SDL_GL_BLUE_SIZE
3  =: SDL_GL_ALPHA_SIZE
4  =: SDL_GL_BUFFER_SIZE
5  =: SDL_GL_DOUBLEBUFFER
6  =: SDL_GL_DEPTH_SIZE
7  =: SDL_GL_STENCIL_SIZE
8  =: SDL_GL_ACCUM_RED_SIZE
9  =: SDL_GL_ACCUM_GREEN_SIZE
10 =: SDL_GL_ACCUM_BLUE_SIZE
11 =: SDL_GL_ACCUM_ALPHA_SIZE
12 =: SDL_GL_STEREO
13 =: SDL_GL_MULTISAMPLEBUFFERS
14 =: SDL_GL_MULTISAMPLESAMPLES
15 =: SDL_GL_ACCELERATED_VISUAL
16 =: SDL_GL_SWAP_CONTROL

\ Everything else I have not looked into

$01     =: SDL_APPMOUSEFOCUS
$02     =: SDL_APPINPUTFOCUS
$04     =: SDL_APPACTIVE

99      =: SDL_MAX_TRACKS
$04     =: SDL_DATA_TRACK

1       =: SDL_HAS_64BIT_TYPE
1       =: STDC_HEADERS

1       =: SDL_ASSEMBLY_ROUTINES
1       =: SDL_HERMES_BLITTERS
1       =: SDL_ALTIVEC_BLITTERS

0       =: SDL_IGNORE
0       =: SDL_DISABLE
1       =: SDL_ENABLE

0       =: SDL_RELEASED
1       =: SDL_PRESSED

1       =: SDL_MUTEX_TIMEDOUT
1       =: NeedFunctionPrototypes
0       =: RW_SEEK_SET
1       =: RW_SEEK_CUR
2       =: RW_SEEK_END

\ ---[ Event enumerations ]------------------------------------------

 0 =: SDL_NOEVENT                            \ Unused (do not remove)
 1 =: SDL_ACTIVEEVENT            \ Application loses/gains visibility
 2 =: SDL_KEYDOWN                                      \ Keys pressed
 3 =: SDL_KEYUP                                       \ Keys released
 4 =: SDL_MOUSEMOTION                                   \ Mouse moved
 5 =: SDL_MOUSEBUTTONDOWN                      \ Mouse button pressed
 6 =: SDL_MOUSEBUTTONUP                       \ Mouse button released
 7 =: SDL_JOYAXISMOTION                        \ Joystick axis motion
 8 =: SDL_JOYBALLMOTION                   \ Joystick trackball motion
 9 =: SDL_JOYHATMOTION                 \ Joystick hat position change
10 =: SDL_JOYBUTTONDOWN                     \ Joystick button pressed
11 =: SDL_JOYBUTTONUP                      \ Joystick button released
12 =: SDL_QUIT                                  \ User-requested quit
13 =: SDL_SYSWMEVENT                          \ System specific event
14 =: SDL_EVENT_RESERVEDA                 \ Reserved for future use..
15 =: SDL_EVENT_RESERVEDB                 \ Reserved for future use..
16 =: SDL_VIDEORESIZE                       \ User resized video mode
17 =: SDL_VIDEOEXPOSE                    \ Screen needs to be redrawn
18 =: SDL_EVENT_RESERVED2                 \ Reserved for future use..
19 =: SDL_EVENT_RESERVED3                 \ Reserved for future use..
20 =: SDL_EVENT_RESERVED4                 \ Reserved for future use..
21 =: SDL_EVENT_RESERVED5                 \ Reserved for future use..
22 =: SDL_EVENT_RESERVED6                 \ Reserved for future use..
23 =: SDL_EVENT_RESERVED7                 \ Reserved for future use..

\ Events SDL_USEREVENT through SDL_MAXEVENTS-1 are for your use

24 =: SDL_USEREVENT

\ This last event is only for bounding internal arrays
\ It is the number of bits in the event mask datatype -- Uint32

32 =: SDL_NUMEVENTS

\ ---[ Predefined event masks ]--------------------------------------

: SDL-EventMask { _x -- 1<<_x } 1 _x LSHIFT ;

SDL_ACTIVEEVENT     SDL-EventMask    =: SDL_ACTIVEEVENTMASK
SDL_KEYDOWN         SDL-EventMask    =: SDL_KEYDOWNMASK
SDL_KEYUP           SDL-EventMask    =: SDL_KEYUPMASK

SDL_KEYDOWNMASK SDL_KEYUPMASK OR     =: SDL_KEYEVENTMASK

SDL_MOUSEMOTION     SDL-EventMask    =: SDL_MOUSEMOTIONMASK
SDL_MOUSEBUTTONDOWN SDL-EventMask    =: SDL_MOUSEBUTTONDOWNMASK
SDL_MOUSEBUTTONUP   SDL-EventMask    =: SDL_MOUSEBUTTONUPMASK

SDL_MOUSEMOTIONMASK
SDL_MOUSEBUTTONDOWNMASK OR
SDL_MOUSEBUTTONUPMASK OR             =: SDL_MOUSEEVENTMASK

SDL_JOYAXISMOTION   SDL-EventMask    =: SDL_JOYAXISMOTIONMASK
SDL_JOYBALLMOTION   SDL-EventMask    =: SDL_JOYBALLMOTIONMASK
SDL_JOYHATMOTION    SDL-EventMask    =: SDL_JOYHATMOTIONMASK
SDL_JOYBUTTONDOWN   SDL-EventMask    =: SDL_JOYBUTTONDOWNMASK
SDL_JOYBUTTONUP     SDL-EventMask    =: SDL_JOYBUTTONUPMASK

SDL_JOYAXISMOTIONMASK
SDL_JOYBALLMOTIONMASK OR
SDL_JOYHATMOTIONMASK OR
SDL_JOYBUTTONDOWNMASK OR
SDL_JOYBUTTONUPMASK OR               =: SDL_JOYEVENTMASK

SDL_VIDEORESIZE     SDL-EventMask    =: SDL_VIDEORESIZEMASK
SDL_VIDEOEXPOSE     SDL-EventMask    =: SDL_VIDEOEXPOSEMASK
SDL_QUIT            SDL-EventMask    =: SDL_QUITMASK
SDL_SYSWMEVENT      SDL-EventMask    =: SDL_SYSWMEVENTMASK

-1 =: SDL_ALLEVENTS
