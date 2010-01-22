\ $Id: colon.fs 86 2009-12-11 16:32:26Z sd $

\ This file is part of Attila, a retargetable threaded interpreter
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

\ The cross-compiler colon-definition compiler, plus other helpers

<WORDLISTS ALSO CROSS-COMPILER DEFINITIONS

\ Look up a word's txt in the target
: FIND \ ( addr n -- 0 | xt 1 | xt -1 )
    TARGET-WORDLIST SEARCH-WORDLIST
    DUP IF
	DROP EXECUTE TARGET-XT
	1 OVER [CROSS] IMMEDIATE? IF NEGATE THEN   \ check immediacy on target
    THEN ;

\ Look up a word in the target, failing if we don't find it
: (') \ ( addr n -- txt )
    2DUP [CROSS-COMPILER] FIND IF
	ROT 2DROP
    ELSE
	TYPE S" ?" ABORT
    THEN ;

\ Look up the next word in the input stream in the target
: ' \ ( "name" -- txt )
    PARSE-WORD [CROSS-COMPILER] (') ;

\ Include using the underlying operation
: INCLUDE [FORTH] INCLUDE ;

\ Postpone dynamically
: POSTPONE
    [CROSS-COMPILER] ' [CROSS] CTCOMPILE, ; IMMEDIATE

WORDLISTS>


\ ---------- Cross-compiling words ----------

<WORDLISTS ONLY FORTH ALSO CROSS DEFINITIONS

\ Compile the code needed to cross-compile at run-time the next
\ word in the input stream
: [COMPILE] \ ( "name" -- )
    PARSE-WORD POSTPONE SLITERAL
    [ 'CROSS-COMPILER (') ] LITERAL CTCOMPILE,
    [ 'CROSS CTCOMPILE, ] LITERAL CTCOMPILE, ; IMMEDIATE

\ Compile the txt of the next word as a literal. The lookup is done at
\ run-time, not (as is usual) at compile-time, so that control words can be
\ defined before the image is populated
: ['] \ ( "name" -- )
    PARSE-WORD POSTPONE SLITERAL
    [ 'CROSS-COMPILER (') ] LITERAL CTCOMPILE, ; IMMEDIATE

WORDLISTS>


\ ---------- Literals ----------

<WORDLISTS ALSO CROSS DEFINITIONS
\ Compile the top of the stack as a literal
: LITERAL \ ( n -- )
    [CROSS] [COMPILE] (LITERAL)
    [CROSS] COMPILE, ; IMMEDIATE

\ Compile the address on the top of the stack as a literal
: ALITERAL \ ( addr -- )
    [CROSS] [COMPILE] (LITERAL)
    [CROSS] ACOMPILE, ; IMMEDIATE

\ Compile the xt on the top of the stack as a literal
: XTLITERAL \ ( xt -- )
    [CROSS] [COMPILE] (LITERAL)
    [CROSS] XTCOMPILE, ; IMMEDIATE

\ Compile the cfa on the top of the stack as a literal
: CFALITERAL \ ( cfa -- )
    [CROSS] [COMPILE] (LITERAL)
    [CROSS] CFACOMPILE, ; IMMEDIATE

\ Compile the string on the top of the stack as a literal
: SLITERAL \ ( addr len -- )
    [CROSS] [COMPILE] (SLITERAL)
    [CROSS] SCOMPILE, ; IMMEDIATE

\ Compile a "-delimited string from the input source as a literal
: S" \ ( "string" -- )
    32 CONSUME \ spaces
    [CHAR] " PARSE
    ?DUP IF
	[ 'CROSS SLITERAL CTCOMPILE, ]
    ELSE
	S" String not delimited" ABORT
    THEN ; IMMEDIATE
WORDLISTS>

<WORDLISTS ALSO CROSS-COMPILER DEFINITIONS

\ Cross-compile the txt of the next word as a literal
: ['] \ ( "name" -- )
    [CROSS-COMPILER] '
    [ 'CROSS XTLITERAL CTCOMPILE, ] ; IMMEDIATE 

WORDLISTS>


\ ---------- The colon cross-compiler ----------

<WORDLISTS ALSO CROSS-COMPILER DEFINITIONS

\ Make the last word defined IMMEDIATE
: IMMEDIATE \ ( -- )
    IMMEDIATE-MASK [CROSS] LASTXT [CROSS] SET-STATUS ;

\ Make the last word defined REDIRECTABLE
: REDIRECTABLE \ ( -- )
    REDIRECTABLE-MASK [CROSS] LASTXT [CROSS] SET-STATUS ; 

\ The colon-compiler automaton 
\ Process a single token depending on the state we're in  
: CROSS-COMPILE1
    2DUP [CROSS-COMPILER] FIND CASE
	1 OF
	    ROT 2DROP
	    [CROSS] CTCOMPILE,
	ENDOF
	1 NEGATE OF
	    DROP
	    2DUP FIND
	    CASE
		1 NEGATE OF
		    ROT ( 2DROP ) TYPE SPACE ." shadowed by host, xt=" DUP . CR
		    EXECUTE
		ENDOF
		1 OF
		    DROP TYPE SPACE S" is not IMMEDIATE on host" ABORT
		ENDOF
		0 OF
		    TYPE SPACE S" has no host analogue" ABORT
		ENDOF
	    ENDCASE
	ENDOF
	0 OF
	    2DUP FIND CASE
		1 OF
		    DROP TYPE SPACE S" not shadowed by host" ABORT
		ENDOF
		1 NEGATE OF
		    ROT 2DROP \ TYPE SPACE ." executed from host" CR
		    EXECUTE
		ENDOF
		0 OF
		    2DUP NUMBER? IF
			ROT 2DROP
			[ 'CROSS LITERAL CTCOMPILE, ]
		    ELSE
			TYPE S" ?" ABORT
		    THEN
		ENDOF
	    ENDCASE
	ENDOF
    ENDCASE ;
: INTERPRET1
    2DUP FIND IF
	ROT 2DROP
	EXECUTE
    ELSE
	2DUP NUMBER? IF
	    ROT 2DROP
	ELSE
	    TYPE S" ?" ABORT
	THEN
    THEN ;

\ The action taken for the next token read by the colon-compiler
VARIABLE NEXT-:-ACTION

\ Set the next action to (cross)-compiling
: ] [ 'CROSS-COMPILER CROSS-COMPILE1 ] LITERAL
    [CROSS-COMPILER] NEXT-:-ACTION XT!
    ]
    CROSS-COMPILATION-STATE STATE ! ;

\ Set the next action to interpreting
: [ [ 'CROSS-COMPILER INTERPRET1 ] LITERAL [CROSS-COMPILER] NEXT-:-ACTION XT!
    POSTPONE [
    INTERPRETATION-STATE STATE ! ; IMMEDIATE

\ The colon-definer
: : \ ( "name" -- txt )
    PARSE-WORD
    2DUP [CROSS] ['] (:) [CROSS] CFA@ [CROSS] (HEADER,)
    (WORD-LOCATOR)
    [CROSS-COMPILER] ]
    BEGIN
	[CROSS-COMPILER] NEXT-:-ACTION XT@ ?DUP
    WHILE
	    PARSE-WORD -ROT EXECUTE
    REPEAT ;

\ Complete a colon-definition
: ; \ ( txt -- )
    [CROSS] NEXT,
    [ 'CROSS-COMPILER [ CTCOMPILE, ]
    0 [CROSS-COMPILER] NEXT-:-ACTION !       \ exit the automaton
    DROP ; IMMEDIATE

\ Compile an anonymous word with no name, which will leave its txt
\ on the data stack when completed with ;
: :NONAME \ ( -- xt xt )
    NULLSTRING [CROSS] ['] (:) [CROSS] CFA@ [CROSS] (HEADER,)
    DUP
    [CROSS-COMPILER] ]
    BEGIN
	[CROSS-COMPILER] NEXT-:-ACTION XT@ ?DUP
    WHILE
	    PARSE-WORD -ROT EXECUTE
    REPEAT ;

WORDLISTS>


