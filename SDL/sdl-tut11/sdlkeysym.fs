\ ===[ Code Addendum 05 ]============================================
\                 gforth: SDL/OpenGL Graphics Part XI
\ ===================================================================
\    File Name: sdlkeysym.fs
\       Author: Timothy Trussell
\         Date: 05/09/2010
\  Description: Define SDL Keyboard Symbol constants
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-21
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\                    Conversion of SDL_keysym.h 
\ ===================================================================

\ The keyboard syms have been cleverly chosen to map to ASCII

  0 constant SDLK_UNKNOWN
  0 constant SDLK_FIRST
  8 constant SDLK_BACKSPACE
  9 constant SDLK_TAB
 12 constant SDLK_CLEAR
 13 constant SDLK_RETURN
 19 constant SDLK_PAUSE
 27 constant SDLK_ESCAPE
 32 constant SDLK_SPACE
 33 constant SDLK_EXCLAIM
 34 constant SDLK_QUOTEDBL
 35 constant SDLK_HASH
 36 constant SDLK_DOLLAR
 38 constant SDLK_AMPERSAND
 39 constant SDLK_QUOTE
 40 constant SDLK_LEFTPAREN
 41 constant SDLK_RIGHTPAREN
 42 constant SDLK_ASTERISK
 43 constant SDLK_PLUS
 44 constant SDLK_COMMA
 45 constant SDLK_MINUS
 46 constant SDLK_PERIOD
 47 constant SDLK_SLASH
 48 constant SDLK_0
 49 constant SDLK_1
 50 constant SDLK_2
 51 constant SDLK_3
 52 constant SDLK_4
 53 constant SDLK_5
 54 constant SDLK_6
 55 constant SDLK_7
 56 constant SDLK_8
 57 constant SDLK_9
 58 constant SDLK_COLON
 59 constant SDLK_SEMICOLON
 60 constant SDLK_LESS
 61 constant SDLK_EQUALS
 62 constant SDLK_GREATER
 63 constant SDLK_QUESTION
 64 constant SDLK_AT

\ Skip uppercase letters

 91 constant SDLK_LEFTBRACKET
 92 constant SDLK_BACKSLASH
 93 constant SDLK_RIGHTBRACKET
 94 constant SDLK_CARET
 95 constant SDLK_UNDERSCORE
 96 constant SDLK_BACKQUOTE
 97 constant SDLK_a
 98 constant SDLK_b
 99 constant SDLK_c
100 constant SDLK_d
101 constant SDLK_e
102 constant SDLK_f
103 constant SDLK_g
104 constant SDLK_h
105 constant SDLK_i
106 constant SDLK_j
107 constant SDLK_k
108 constant SDLK_l
109 constant SDLK_m
110 constant SDLK_n
111 constant SDLK_o
112 constant SDLK_p
113 constant SDLK_q
114 constant SDLK_r
115 constant SDLK_s
116 constant SDLK_t
117 constant SDLK_u
118 constant SDLK_v
119 constant SDLK_w
120 constant SDLK_x
121 constant SDLK_y
121 constant SDLK_z
127 constant SDLK_DELETE
\ End of ASCII mapped keysyms

\ International keyboard syms
160 constant SDLK_WORLD_0               \ 0xA0
161 constant SDLK_WORLD_1
162 constant SDLK_WORLD_2
163 constant SDLK_WORLD_3
164 constant SDLK_WORLD_4
165 constant SDLK_WORLD_5
166 constant SDLK_WORLD_6
167 constant SDLK_WORLD_7
168 constant SDLK_WORLD_8
169 constant SDLK_WORLD_9
170 constant SDLK_WORLD_10
171 constant SDLK_WORLD_11
172 constant SDLK_WORLD_12
173 constant SDLK_WORLD_13
174 constant SDLK_WORLD_14
175 constant SDLK_WORLD_15
176 constant SDLK_WORLD_16
177 constant SDLK_WORLD_17
178 constant SDLK_WORLD_18
179 constant SDLK_WORLD_19
180 constant SDLK_WORLD_20
181 constant SDLK_WORLD_21
182 constant SDLK_WORLD_22
183 constant SDLK_WORLD_23
184 constant SDLK_WORLD_24
185 constant SDLK_WORLD_25
186 constant SDLK_WORLD_26
187 constant SDLK_WORLD_27
188 constant SDLK_WORLD_28
189 constant SDLK_WORLD_29
190 constant SDLK_WORLD_30
191 constant SDLK_WORLD_31
192 constant SDLK_WORLD_32
193 constant SDLK_WORLD_33
194 constant SDLK_WORLD_34
195 constant SDLK_WORLD_35
196 constant SDLK_WORLD_36
197 constant SDLK_WORLD_37
198 constant SDLK_WORLD_38
199 constant SDLK_WORLD_39
200 constant SDLK_WORLD_40
201 constant SDLK_WORLD_41
202 constant SDLK_WORLD_42
203 constant SDLK_WORLD_43
204 constant SDLK_WORLD_44
205 constant SDLK_WORLD_45
206 constant SDLK_WORLD_46
207 constant SDLK_WORLD_47
208 constant SDLK_WORLD_48
209 constant SDLK_WORLD_49
210 constant SDLK_WORLD_50
211 constant SDLK_WORLD_51
212 constant SDLK_WORLD_52
213 constant SDLK_WORLD_53
214 constant SDLK_WORLD_54
215 constant SDLK_WORLD_55
216 constant SDLK_WORLD_56
217 constant SDLK_WORLD_57
218 constant SDLK_WORLD_58
219 constant SDLK_WORLD_59
220 constant SDLK_WORLD_60
221 constant SDLK_WORLD_61
222 constant SDLK_WORLD_62
223 constant SDLK_WORLD_63
224 constant SDLK_WORLD_64
225 constant SDLK_WORLD_65
226 constant SDLK_WORLD_66
227 constant SDLK_WORLD_67
228 constant SDLK_WORLD_68
229 constant SDLK_WORLD_69
230 constant SDLK_WORLD_70
231 constant SDLK_WORLD_71
232 constant SDLK_WORLD_72
233 constant SDLK_WORLD_73
234 constant SDLK_WORLD_74
235 constant SDLK_WORLD_75
236 constant SDLK_WORLD_76
237 constant SDLK_WORLD_77
238 constant SDLK_WORLD_78
239 constant SDLK_WORLD_79
240 constant SDLK_WORLD_80
241 constant SDLK_WORLD_81
242 constant SDLK_WORLD_82
243 constant SDLK_WORLD_83
244 constant SDLK_WORLD_84
245 constant SDLK_WORLD_85
246 constant SDLK_WORLD_86
247 constant SDLK_WORLD_87
248 constant SDLK_WORLD_88
249 constant SDLK_WORLD_89
250 constant SDLK_WORLD_90
251 constant SDLK_WORLD_91
252 constant SDLK_WORLD_92
253 constant SDLK_WORLD_93
254 constant SDLK_WORLD_94
255 constant SDLK_WORLD_95              \ 0xFF

\ Numeric keypad

256 constant SDLK_KP0
257 constant SDLK_KP1
258 constant SDLK_KP2
259 constant SDLK_KP3
260 constant SDLK_KP4
261 constant SDLK_KP5
262 constant SDLK_KP6
263 constant SDLK_KP7
264 constant SDLK_KP8
265 constant SDLK_KP9
266 constant SDLK_KP_PERIOD
267 constant SDLK_KP_DIVIDE
268 constant SDLK_KP_MULTIPLY
269 constant SDLK_KP_MINUS
270 constant SDLK_KP_PLUS
271 constant SDLK_KP_ENTER
272 constant SDLK_KP_EQUALS

\ Arrows + Home/End pad

273 constant SDLK_UP
274 constant SDLK_DOWN
275 constant SDLK_RIGHT
276 constant SDLK_LEFT
277 constant SDLK_INSERT
278 constant SDLK_HOME
279 constant SDLK_END
280 constant SDLK_PAGEUP
281 constant SDLK_PAGEDOWN

\ Function keys

282 constant SDLK_F1
283 constant SDLK_F2
284 constant SDLK_F3
285 constant SDLK_F4
286 constant SDLK_F5
287 constant SDLK_F6
288 constant SDLK_F7
289 constant SDLK_F8
290 constant SDLK_F9
291 constant SDLK_F10
292 constant SDLK_F11
293 constant SDLK_F12
294 constant SDLK_F13
295 constant SDLK_F14
296 constant SDLK_F15

\ Key state modifier keys

300 constant SDLK_NUMLOCK
301 constant SDLK_CAPSLOCK
302 constant SDLK_SCROLLOCK
303 constant SDLK_RSHIFT
304 constant SDLK_LSHIFT
305 constant SDLK_RCTRL
306 constant SDLK_LCTRL
307 constant SDLK_RALT
308 constant SDLK_LALT
309 constant SDLK_RMETA
310 constant SDLK_LMETA
311 constant SDLK_LSUPER        \ Left "Windows" key
312 constant SDLK_RSUPER        \ Right "Windows" key
313 constant SDLK_MODE          \ "Alt Gr" key
314 constant SDLK_COMPOSE       \ Multi-key compose key

\ Miscellaneous function keys

315 constant SDLK_HELP
316 constant SDLK_PRINT
317 constant SDLK_SYSREQ
318 constant SDLK_BREAK
319 constant SDLK_MENU
320 constant SDLK_POWER         \ Power Macintosh power key
321 constant SDLK_EURO          \ Some european keyboards
322 constant SDLK_UNDO          \ Atari keyboard has Undo

\ Add any other keys here

\	SDLK_LAST
\ } SDLKey;

\ Enumeration of valid key mods (possibly OR'd together)

\ typedef enum {

$0000 constant KMOD_NONE
$0001 constant KMOD_LSHIFT
$0002 constant KMOD_RSHIFT
$0040 constant KMOD_LCTRL
$0080 constant KMOD_RCTRL
$0100 constant KMOD_LALT
$0200 constant KMOD_RALT
$0400 constant KMOD_LMETA
$0800 constant KMOD_RMETA
$1000 constant KMOD_NUM
$2000 constant KMOD_CAPS
$4000 constant KMOD_MODE
$8000 constant KMOD_RESERVED

\ } SDLMod;

KMOD_LCTRL  KMOD_RCTRL OR  constant KMOD_CTRL
KMOD_LSHIFT KMOD_RSHIFT OR constant KMOD_SHIFT
KMOD_LALT   KMOD_RALT OR   constant KMOD_ALT
KMOD_LMETA  KMOD_RMETA OR  constant KMOD_META

