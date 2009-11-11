\ $Id: locals-base.fs,v 1.4 2007/06/15 15:57:38 sd Exp $

\ This file is part of Attila, a minimal threaded interpretive language
\ Copyright (c) 2007, UCD Dublin. All rights reserved.
\
\ Attila is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.
\
\ Attila is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.
\
\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

\ Basic local variable definitions
\
\ Local definitions allow Attila words to use named variables internally,
\ rather than explicitly manipulating the data stack. Although some people
\ find the stack liberating, it can be awkward -- especially when targeting
\ code generators at the machine. There is a run-time cost for this
\ convenience, but only when compared to the efficient stack manipulations
\ normally used in stack-based programs: in fact this is pretty much the
\ run-time mechanism used for local values in most "ordinary" languages,
\ except for some extra copying.
\
\ A local definition is essentially a value that is actually stored
\ on a stack (not the data stack) rather than in a fixed memory location.
\ The population of values needed in a definition needs to be created
\ at compile-time and then torn down, so the local symbols can't live in the
\ normal code area.
\
\ The implementation we adopt here is to construct a vocabulary holding
\ local definitions at compile-time. The vocabulary is populated with
\ IMMEDIATE words that expand to fetch values from offsets on the locals stack at
\ run-time. This means that the *execution* of words using locals does not
\ need vocabulary support, although ther *creation* does.
\
\ This file does not provide user-level local definitions. You should also include
\ either ans-locals.fs (ANS standard) or locals.fs (stack-comment style) to get
\ these words
\ Requires stacks.fs, vocabularies.fs, counted-loops.fs


\ The number of locals available within a single word -- re-define with care!
10 VALUE #LOCALS/WORD

\ The total number of locals available at run-time, defaulting to
\ the maximum per word nested 100 deep -- re-define with care!
#LOCALS/WORD 100 * VALUE #LOCALS


\ ---------- Locals run-time ----------
\ sd: should we refactor this into its own file, for cleaner separation?

\ The locals run-time stack
#LOCALS STACK LOCALS-STACK

\ Start a stack frame
: START-LOCALS
    0 LOCALS-STACK >ST ;

\ Create a local
: >LOCAL \ ( n -- )
    LOCALS-STACK ST>
    SWAP LOCALS-STACK >ST
    1+ LOCALS-STACK >ST ;

\ Push the locals onto the locals stack
: >LOCALS \ ( ni ... n1 i -- )
    0 DO
	>LOCAL
    LOOP ;

\ Return the value of the i'th local, counting from 1 (*not* 0 as is usual)
: @LOCAL \ ( n -- v )
    LOCALS-STACK ST-PICK ;

\ Update the value of the i'th local, counting from 1 (*not* 0 as is usual)
: !LOCAL \ ( v n -- )
    LOCALS-STACK ST>ADDR ! ;

\ Clean up the locals stack at the end of a word
: LOCALS> \ ( -- )
    LOCALS-STACK ST> LOCALS-STACK ST-NDROP ;


\ ---------- Locals compile-time ----------

\ The vocabulary used to hold locals at compile-time 
VOCABULARY LOCALS

\ Variable holding the xt of the marker -- horrible horrible...
VARIABLE LOCALS-MARKER

\ Set up to place definitions in the LOCALS vocabulary
: (CREATE-LOCALS-MARKER) \ ( -- )
    CURRENT
    ALSO LOCALS ALSO DEFINITIONS
    S" |" (MARKER) LASTXT LOCALS-MARKER ! ;

\ Create a local. A local is a symbol representing an offset into the
\ stack frame on the locals stack. When executed, it *compiles* the code
\ needed to access the value
: (CREATE-LOCAL) \ ( addr n -- xt )
    (CREATE) 0 ,                  \ create the symbol
    LASTXT DUP (IMMEDIATE)        \ make it IMMEDIATE
    DUP (DOES>)
    @ POSTPONE LITERAL [COMPILE] @LOCAL ;

\ Test whether the gven word identifies a local
\ sd: this will also identify | and ; as locals, which might be undesirable
: (LOCAL?) \ ( addr n -- f )
    ['] LOCALS VOC>LASTHA (FIND) ;


\ ---------- User-level words ----------

\ Update the value of a local. To make this work with VALUEs we look-up the
\ word in LOCALS only and compile different code depending on whether we find
\ it or not
: TO \ ( v "name" -- )
    PARSE-WORD 2DUP (LOCAL?) ?DUP IF
	ROT 2DROP
	>BODY @ POSTPONE LITERAL [COMPILE] !LOCAL
    ELSE
	2DUP FIND IF
	    ROT 2DROP POSTPONE (TO)
	ELSE
	    ABORT
	THEN
    THEN ; IMMEDIATE

