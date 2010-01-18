\ $Id$

\ Architecture definition for the host gcc compiler
\
\ This file is pre-processed to generate the necessary
\ definitions, typically in C. It should contain only
\ simple code such as CONSTANT declarations

\ ---------- Cross-compiler set-up ----------

\ Load the cross-compiler
include cross-compiler.fs

\ Sizes in bytes. These must match the values from the underlying architecture
8 'CROSS /CELL (IS)
1 'CROSS /CHARACTER (IS)

\ Endianness of the processor:
\   0 = little-endian
\   1 = big-endian
0 'CROSS BIGENDIAN? (IS)


\ ---------- The image ----------

.( Initialising image...)
INITIALISE-IMAGE

<TARGET
C: /CELL ( -- bs )
  bs = 8;
;C

C: /CHARACTER ( -- bs )
  bs = 1;
;C

C: CHARACTERS ( n -- bs )
  bs = n ;
;C

C: USERVAR uservar ( n -- addr )
  addr = (CELL) &image[n];
;C
TARGET>

.( Loading target image...)
<TARGET
include core.fs
include itil.fs
include x-lib-fileio-gcc-host.fs
include x-lib-io-gcc-host.fs
include counted-loops-runtime.fs

include base.fs
include flat-memory-model.fs

include itil-compilation.fs
: START-DEFINITION ;
: END-DEFINITION ;
include compilation.fs

include executive.fs
TARGET>

.( Finalising image...)
FINALISE-IMAGE

.( Saving image...)
SAVE-IMAGE-TO test.c

.( Done)

