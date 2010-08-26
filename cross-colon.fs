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

\ The cross-compiler colon-definition compiler, plus other helpers.
\
\ Note that this file is carefully crafted to avoid using the new (cross-compiler)
\ definitions when compiling other cross-compiler definitions. Edit with care!


\ Look up a word's txt in the target
: FIND \ ( addr n -- 0 | xt 1 | xt -1 )
    TARGET-WORDLIST SEARCH-WORDLIST DUP IF
	DROP EXECUTE TARGET-XT [FORTH] @
	1 OVER IMMEDIATE? IF NEGATE THEN   \ check immediacy on target
    THEN ;

\ Look up a word in the target, failing if we don't find it
: (') \ ( addr n -- txt )
    2DUP FIND IF
	ROT 2DROP
    ELSE
	TYPE S" ?" ABORT
    THEN ;

\ Look up the next word in the input stream in the target
: ' ( "name" -- txt )
    PARSE-WORD (') ;

\ Compile the txt of the next word as a literal, but done dynamically
\ so that we get a chance to build the target words first
: ['] ( "name" -- )
    PARSE-WORD POSTPONE SLITERAL
    [FORTH] ['] (') [FORTH] CTCOMPILE, ; [FORTH] IMMEDIATE

\ Compile the code needed to cross-compile at run-time the next
\ word in the input stream
: [COMPILE] \ ( "name" -- )
    PARSE-WORD POSTPONE SLITERAL
    [FORTH] ['] (') [FORTH] CTCOMPILE,
    [FORTH] ['] CTCOMPILE, [FORTH] CTCOMPILE, ; [FORTH] IMMEDIATE


\ ---------- Literals ----------

\ Cross-compile the top of the stack as a literal
: LITERAL \ ( n -- )
    [CROSS-COMPILER-EXT] [COMPILE] (LITERAL)
    COMPILE, ; [FORTH] IMMEDIATE

\ Cross-compile the address on the top of the stack as a literal
: ALITERAL \ ( addr -- )
    [CROSS-COMPILER-EXT] [COMPILE] (LITERAL)
    ACOMPILE, ; [FORTH] IMMEDIATE

\ Cross-compile the xt on the top of the stack as a literal
: XTLITERAL \ ( xt -- )
    [CROSS-COMPILER-EXT] [COMPILE] (LITERAL)
    XTCOMPILE, ; [FORTH] IMMEDIATE

\ Cross-compile the cfa on the top of the stack as a literal
: CFALITERAL \ ( cfa -- )
    [CROSS-COMPILER-EXT] [COMPILE] (LITERAL)
    CFACOMPILE, ; [FORTH] IMMEDIATE

\ Cross-compile the string on the top of the stack as a literal
: SLITERAL \ ( addr len -- )
    [CROSS-COMPILER-EXT] [COMPILE] (SLITERAL)
    SCOMPILE, ; [FORTH] IMMEDIATE


\ ---------- The colon cross-compiler ----------

<CROSS-COMPILER

\ We're cross-compiling if (a) we're compiling and (b) words are going
\ into the TARGET vocabulary
: CROSS-COMPILING?
    INTERPRETING? NOT
    TARGET-WORDLIST [FORTH] CURRENT [FORTH] @ = AND ;

: CROSS-COMPILER-FIND ( addr n -- xt 1 | xt -1 | 0 )
    2DUP CROSS-COMPILER-WORDLIST SEARCH-WORDLIST ?DUP IF
	3 ROLL 3 ROLL 2DROP
    ELSE
	CROSS-COMPILER-EXT-WORDLIST SEARCH-WORDLIST
    THEN ;

\ The cross-executive.
: CROSS-OUTER ( -- )
    BEGIN
	PARSE-WORD ?DUP IF
	    2DUP TYPE CR
	    INTERPRETING? IF
		." interpreting, look the word up using the host FIND" CR
		2DUP FIND IF
		    \ found, execute
		    ROT 2DROP   dup . cr
		    EXECUTE
		ELSE
		    \ no found, is it a number?
		    2DUP NUMBER? IF
			\ a number, leave on the stack
			ROT 2DROP
		    ELSE
			\ not a number, fail
			TYPE S" ?" ABORT
		    THEN
		THEN
	    ELSE
		." not interpreting, are we compiling or cross-compiling?" CR
		[CROSS-COMPILER] CROSS-COMPILING? IF
		    ." cross-compiling, lookup using cross-compiler" CR
		    2DUP [CROSS-COMPILER-EXT] FIND CASE
			1 OF
			    \ found, compile
			    ROT 2DROP
			    [CROSS] CTCOMPILE,
			ENDOF
			1 NEGATE OF
			    ." found an immediate, look for shadow" CR
			    DROP
			    2DUP [CROSS-COMPILER] CROSS-COMPILER-FIND CASE
				1 OF
				    \ found but not immediate, fail
				    DROP
				    TYPE SPACE S" is shadowed but not IMMEDIATE on host" ABORT
				ENDOF
				1 NEGATE OF
				    ." found and immediate, execute" CR
				    ROT 2DROP
				    EXECUTE
				ENDOF
				0 OF
				    \ not found, fail
				    TYPE S" ?" ABORT
				ENDOF
			    ENDCASE
			ENDOF
			0 OF
			    ." not found, is the word a cross-compiler immediate word?" CR
			    2DUP [CROSS-COMPILER] CROSS-COMPILER-FIND CASE
				1 OF
				    \ found but not immediate, abort
				    DROP
				    TYPE SPACE S" is not a cross-compiler word" ABORT
				ENDOF
				1 NEGATE OF
				    \ found an immediate, execute
				    ROT 2DROP
				    EXECUTE
				ENDOF
				0 OF
				    \ not found, is it a number?
				    2DUP NUMBER? IF
					." a number, compile as a literal" CR
					ROT 2DROP
					[ 'CROSS-COMPILER-EXT LITERAL [FORTH] CTCOMPILE, ] \ POSTPONE [CROSS-COMPILER-EXT] LITERAL
				    ELSE
					\ not a number, fail
					TYPE S" ?" ABORT
				    THEN
				ENDOF
			    ENDCASE
			ENDOF
		    ENDCASE
		ELSE
		    ." compiling, as normal" CR
		    2DUP FIND CASE
			1 OF
			    \ found, compile
			    ROT 2DROP
			    [FORTH] CTCOMPILE,
			ENDOF
			1 NEGATE OF
			    \ found an immediate, execute
			    ROT 2DROP
			    EXECUTE
			ENDOF
			0 OF
			    \ not found, is it a number?
			    2DUP NUMBER? IF
				\ number, compile as literal
				ROT 2DROP
				POSTPONE LITERAL
			    ELSE
				\ not a number, fail
				TYPE S" ?" ABORT
			    THEN
			ENDOF
		    ENDCASE
		THEN
	    THEN
	THEN
    EXHAUSTED? UNTIL ;

\ Cross-compile a "-delimited string from the input source as a literal
: S" \ ( "string" -- )
    32 CONSUME \ spaces
    [FORTH] [CHAR] " PARSE
    ?DUP IF
	POSTPONE SLITERAL
    ELSE
	S" String not delimited" ABORT
    THEN ; [FORTH] IMMEDIATE

\ Compile the next character as a literal
: [CHAR] \ ( "name" -- )
    PARSE-WORD CHAR POSTPONE LITERAL ; [FORTH] IMMEDIATE

\ Postpone dynamically
: POSTPONE ( "name" -- )
    ' CTCOMPILE, ; [FORTH] IMMEDIATE


\ The colon cross-compiler

<WORDLISTS ONLY FORTH ALSO CROSS-COMPILER DEFINITIONS

: [ INTERPRETATION-STATE STATE ! ; [FORTH] IMMEDIATE
: ] COMPILATION-STATE    STATE ! ;

WORDLISTS>

: COLON ( addr n -- txt )
    [CROSS-COMPILER-EXT] ['] (:) [CROSS] CFA@ [CROSS] (HEADER,)
    ] ;    
: : ( "name" -- txt )
    PARSE-WORD [CROSS-COMPILER] COLON ;
: :NONAME ( -- txt txt )
    NULLSTRING [CROSS-COMPILER] COLON DUP ;
: ; ( txt -- )
    NEXT,
    POSTPONE [
    DROP ; [FORTH] IMMEDIATE

CROSS-COMPILER>

<wordlists only forth also cross-compiler definitions

\ Include using the host's include operations wrapped-up with a new executive
: INCLUDED ( addr n -- )
    EXECUTIVE @ ROT
    [ 'CROSS-COMPILER CROSS-OUTER ] LITERAL EXECUTIVE !
    2DUP ." include " TYPE CR
    INCLUDED
    EXECUTIVE ! ;
: INCLUDE ( "name" -- )
    PARSE-WORD [CROSS-COMPILER] INCLUDED ;

wordlists>
