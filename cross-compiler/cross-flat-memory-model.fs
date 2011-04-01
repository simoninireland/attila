\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
\ Copyright (c) 2007--2010, Simon Dobson <simon.dobson@computer.org>.
\ All rights reserved.
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

\ Cross-compiler header compiler for flat memory model with a single
\ linear code and data allocation.

\ ---------- User variables ----------

\ User variables live in the bottom cells of the image
: (SIMPLE-USERVAR)
    /CELL * ;

' (SIMPLE-USERVAR) IS USERVAR


\ ---------- Platform and compilation primitives ----------

\ HERE and TOP are the same in this model, as are THERE and CEILING
: TOP     (TOP) A@ ;
: CEILING (CEILING) A@ ;
: HERE    TOP ;
: THERE   CEILING ;
: LASTXT  LAST XT@ ;

\ Compile a character
: CCOMPILE, \ ( c -- )
    TOP C!
    /CHAR (TOP) A+! ;

\ Align code address on cell boundaries
: CALIGN T>ALIGN ;
: CALIGNED \ ( -- )
    TOP CALIGN TOP - ?DUP IF
	0 DO
	    0 CCOMPILE,
	LOOP
    THEN ;
: CALIGNED? \ ( addr -- f )
    DUP CALIGN = ;

\ Compile value
: COMPILE, \ ( n -- )
    CALIGNED
    TOP !
    /CELL (TOP) A+! ;

\ Compile real (target) address
: RACOMPILE,
    CALIGNED
    TOP RA!
    /CELL (TOP) A+! ;

\ Compile Forth address
: ACOMPILE, \ ( taddr -- )
    CALIGNED
    TOP A!
    /CELL (TOP) A+! ;

\ Compile  target xt
: XTCOMPILE,
    CALIGNED
    TOP XT!
    /CELL (TOP) A+! ;

\ Compile CFA
: CFACOMPILE, \ ( cf -- )
    CALIGNED
    TOP (CFA!)
    /CELL (TOP) A+! ;

\ In this model, data and code compilation are the same
: C,       CCOMPILE, ;
: ,        COMPILE, ;
: RA,      RACOMPILE, ;
: A,       ACOMPILE, ;
: XT,      XTCOMPILE, ;
: ALIGN    CALIGN ;
: ALIGNED  CALIGNED ;
: ALIGNED? CALIGNED? ;

\ Strings need to be copied from host memory, hence the explicit use of host C@ and CHARS
: CSEQCOMPILE, \ ( addr n -- )
    ?DUP 0> IF
	1+ 0 DO
	    DUP [FORTH] C@ CCOMPILE,
	    [FORTH] /CHAR +
	LOOP
    THEN
    DROP CALIGNED ;
: SCOMPILE, \ ( addr n -- )
    CALIGNED DUP CCOMPILE, CSEQCOMPILE, ;
: S, \ ( addr n -- )
    ALIGNED
    DUP C,
    1+ 0 DO
	DUP [FORTH] C@ C,
	[FORTH] /CHAR +
    LOOP DROP ;

\ Similarly for memory movements -- from host addr to target taddr
: CMOVE ( addr taddr n -- )
    1+ 0 DO
	OVER [FORTH] C@ OVER C!
	/CHAR + SWAP [FORTH] /CHAR + SWAP
    LOOP 2DROP ;
: CMOVE> CMOVE ;

\ ALLOTting data space simply data-compiles zeros
: ALLOT ( n -- )
    1+ 0 DO
	0 C,
    LOOP ;


\ ---------- High-level image initialisation and finalisation ----------

\ Initialise the image, recording its name
: INITIALISE-IMAGE ( addr n -- )
    [CROSS] IMAGE-SIZE (INITIALISE-IMAGE)

    \ allocate user variables
    USER-SIZE 0 DO
	0 I /CELL * !
    LOOP

    \ initialise core user variables
    USER-SIZE  /CELL * (TOP)     A!
    IMAGE-SIZE /CELL * (CEILING) A!
    0 LAST A!

    \ store the image name
    ALIGNED HERE (IMAGE-NAME) A!
    S,
    
    \ initialise stacks and TIB
    ALIGNED
    HERE (DATA-STACK)       A!      \ data stack 
    DATA-STACK-SIZE CELLS   ALLOT
    ALIGNED
    HERE (RETURN-STACK)     A!      \ return stack 
    RETURN-STACK-SIZE CELLS ALLOT
    ALIGNED
    HERE TIB                A!      \ terminal input buffer
    TIB-SIZE                ALLOT
    ALIGNED ;

\ Finalise the image prior to being output
: FINALISE-IMAGE
    (FINALISE-IMAGE) ;
