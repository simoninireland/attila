\ $Id: ans-locals.fs,v 1.2 2007/05/24 21:51:03 sd Exp $

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

\ ANS standard locals words
\
\ The ANS standard defines the words (LOCAL), LOCALS| and TO as the compile-time
\ interface to lcoal definitions. Many people regard the definition of LOCALS|
\ wrong and limited, but if you need standard locals, include this file.
\ Requires locals-base.fs


\ Override ; in the LOCALS vocabulary, having it compile the code
\ to off-load the values on the locals stack
\ sd: This is a horrible hack and completely non-standard as it
\ doesn't work with exceptions etc -- it badly needs re-writing
CURRENT
ALSO LOCALS ALSO DEFINITIONS
: ;
    LOCALS-MARKER @ EXECUTE                \ forget the local symbols
    PREVIOUS                               \ into previous environment
    [COMPILE] LOCALS>                      \ compile code to clean up locals stack
    [ PREVIOUS ] POSTPONE ; ; IMMEDIATE    \ do the normal ; behaviour
(DEFINITIONS)

\ The standard "create a local" message. If n > 0 we create the local; if
\ n = 0 we do nothing
: (LOCAL) \ ( addr n | 0 -- )
    ?DUP IF
	(CREATE-LOCAL) DROP
    ELSE
	DROP
    THEN ;

\ Parse a list of names up to the closing |, creating locals for each.
\ sd: This is the ANS standard way, and is truly awful: the word itself is
\ too long, and the order of names is reversed compared to what one would
\ reasonably expect in terms of stack comments etc. We provide it purely for
\ compliance
: LOCALS| \ ( "n1" ... "ni" "|" -- )
    \ define each local
    (CREATE-LOCALS-MARKER) 0
    BEGIN
	PARSE-WORD
	2DUP S" |" S<>
    WHILE
	    (LOCAL) LASTXT SWAP 1+
    REPEAT 2DROP
    0 0 (LOCAL)
    
    \ patch each local with its run-time offset on the locals stack
    DUP 0 DO
	\ DUP I - -ROT >BODY !
	I 1+ -ROT >BODY !
    LOOP

    \ leave the locals vocabulary on the top of the search order, restoring
    \ the previous current vocabulary
    \ sd: this is not quite the correct behaviour, as we should insulate
    \ against vocabulary changes within the word's definition in order
    \ to *keep* LOCALS on top. Perhaps we should be able to "pin" vocabularies
    \ to the top of the search order within vocabularies.fs?
    SWAP (DEFINITIONS)

    \ compile the code to set up the locals at run-time
    [COMPILE] START-LOCALS
    POSTPONE LITERAL [COMPILE] >LOCALS ; IMMEDIATE

