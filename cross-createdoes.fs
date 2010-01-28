\ $Id$

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

\ Cross-compiler CREATE ... DOES>
\
\ This allows code that uses CREATE...DOES> to be loaded into CROSS,
\ and to cross-compile its behaviour into the target.

<WORDLISTS ALSO CROSS DEFINITIONS

\ ---------- CREATE ----------

\ Create a data block that returns its body address when executed
: (DATA) \ ( addr n -- )
    2DUP [CROSS] ['] (VAR) [CROSS] CFA@ [CROSS] (HEADER,)
    (WORD-LOCATOR) DROP ; \ create a locator for the new word
    
\ Create a data block from the next word in the input
: DATA \ ( "name" -- )
    PARSE-WORD [CROSS] (DATA) ;

\ Create a data block that can have its run-time behaviour re-directed
: (CREATE) \ ( addr n -- )
    [CROSS] (DATA)
    1 [CROSS] CELLS [CROSS] ALLOT    \ the indirect body address (iba)
    [CROSS-COMPILER] REDIRECTABLE ;  \ fix status
    

\ ---------- DOES> ----------

<WORDLISTS ALSO CROSS-COMPILER DEFINITIONS 

\ For code loaded into the target: cross-compile the code to
\ set the IBA of the created word
\
\ (DOES) is provided by the inner interpreter
\ (DOES>) is assumed to be available on the target

\ Create a data block from the next word in the input
: CREATE \ ( "name" -- )
    PARSE-WORD (CREATE) ;

\ Compile the setting behaviour
: DOES> \ ( -- )
    [CROSS] ['] LASTXT  [CROSS] CTCOMPILE,
    [CROSS] ['] (DOES>) [CROSS] CTCOMPILE, ; IMMEDIATE

WORDLISTS>


\ For code loaded into the cross-compiler: set the IBA of the
\ created word be the same as that which would be set by the
\ defining word of the same name on the target.
\
\ The idea here is that we can load the same code into the
\ target and the cross-compiler, so that the target code can
\ use its defining words. The CREATE part will run on the
\ host and create new words on the target as expected; the
\ DOES> part will set the word to use the right target code
\ when it is executed.

\ Create a data block from the next word in the input
: CREATE \ ( "name" -- )
    ." CCC"
    PARSE-WORD [CROSS] (CREATE) ;

\ Find the target address of the code following (DOES) in the target word
: FIND-IBA \ ( txt -- taddr )
    [CROSS] >BODY
    BEGIN
	DUP [CROSS] XT@
	[CROSS] ['] (DOES) <>
    WHILE
	    1 CELLS +
    REPEAT
    1 CELLS + ;

\ Set the CFA and IBA of the created target word
: (DOES>) \ ( taddr txt -- )
    [CROSS] ['] (DOES) [CROSS] CFA@ OVER [CROSS] CFA!  \ cfa
    IBA!                                                        \ iba
    R> DROP ;                                                   \ jump out

\ Compile the code to cross-compile the behaviour. Note that we still
\ compile (on the host) the run-time behaviour to ensure that any
\ side-effects are executed
: DOES> \ ( -- )
    LASTXT >NAME [CROSS-COMPILER] (') [CROSS] FIND-IBA [ 'CROSS-COMPILER LITERAL CTCOMPILE, ]
    [ 'CROSS LASTXT ] LITERAL CTCOMPILE,
    [ 'CROSS (DOES>) ] LITERAL CTCOMPILE, ; IMMEDIATE

WORDLISTS>
