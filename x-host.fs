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

<WORDLISTS ALSO CROSS-COMPILER DEFINITIONS
: CONSTANT
    CREATE ,
  DOES>
    @
    INTERPRETING? NOT IF
	[ 'CROSS-COMPILER LITERAL CTCOMPILE, ]
    THEN ;
: USER
    CREATE ,
  DOES>
    @
    INTERPRETING? NOT IF
	[ 'CROSS-COMPILER LITERAL CTCOMPILE, ]
	[CROSS] [COMPILE] USERVAR
    THEN ;
WORDLISTS>

.( Initialising image...)
INITIALISE-IMAGE

.( Adding stacks)
20 [CROSS] CELLS [CROSS] ALLOT
20 [CROSS] CELLS [CROSS] ALLOT

.( Adding TIB)
800 [CROSS] ALLOT

.( Loading target basic image...)
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

include c-core.fs
include c-itil.fs
TARGET>

.( Loading cross-compiler control structures...)
<WORDLISTS ALSO CROSS ALSO DEFINITIONS
include chars.fs
include cross-cs.fs
WORDLISTS>
<WORDLISTS ALSO CROSS ALSO CROSS-COMPILER DEFINITIONS
include conditionals.fs
include loops.fs
include counted-loops.fs
WORDLISTS>

.( Loading rest of image...) 
<TARGET
include ascii.fs
include c-fileio.fs
include c-io.fs
include counted-loops-runtime.fs

include base.fs
include flat-memory-model.fs
include chars.fs

include itil-compilation.fs
: START-DEFINITION ;
: END-DEFINITION ;
include compilation.fs
include colon.fs

include executive.fs
include file.fs
include load.fs
TARGET>

.( Finalising image...)
FINALISE-IMAGE
[CROSS-COMPILER] ' COLD 0 [CROSS] USERVAR [CROSS] XT!   \ cold-start vector 
[CROSS-COMPILER] ' OUTER 1 [CROSS] USERVAR [CROSS] XT!  \ outer executive
100 [CROSS] /CELL * 9 [CROSS] USERVAR [CROSS] A!        \ TIB
[CROSS] LASTXT 10 [CROSS] USERVAR [CROSS] XT!           \ LAST

.( Saving image...)
SAVE-IMAGE-TO test.c

.( Done)

