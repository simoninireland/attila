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

\ The cross-compiler colon-definition compiler, plus other helpers.
\
\ Note that this file is carefully crafted to avoid using the new (cross-compiler)
\ definitions when compiling other cross-compiler definitions. Edit with care!

\ ---------- Cross-compiler testing ----------

\ We're cross-compiling if (a) we're compiling and (b) words are going
\ into the TARGET vocabulary
: CROSS-COMPILING? ( -- f )
    INTERPRETING? NOT
    TARGET-WORDLIST [FORTH] CURRENT [FORTH] @ = AND ;


\ ---------- Finding ----------

\ Look up a word's txt in the target
: FIND \ ( addr n -- 0 | xt 1 | xt -1 )
    TARGET-WORDLIST SEARCH-WORDLIST DUP IF
	DROP EXECUTE TARGET-XT [FORTH] @
	1 OVER IMMEDIATE? IF NEGATE THEN   \ check immediacy on target
    THEN ;

\ Look up a word in the target, failing if we don't find it
: (') \ ( addr n -- txt )
    2DUP [cross-compiler] FIND IF
	ROT 2DROP
    ELSE
	TYPE S" ?" ABORT
    THEN ;

\ Look up the next word in the input stream in the target
: ' ( "name" -- txt )
    PARSE-WORD [CROSS-COMPILER] (') ;

\ Compile the txt of the next word as a literal, but done dynamically
\ so that we get a chance to build the target words first
: ['] ( "name" -- )
    PARSE-WORD
    [CROSS-COMPILER] CROSS-COMPILING? IF
	2DUP [CROSS-COMPILER] FIND IF
	    ROT 2DROP
	    S" (LITERAL)" [CROSS-COMPILER] (') [CROSS] CTCOMPILE, [CROSS] XTCOMPILE,
	ELSE
	    TYPE S" not found on target" ABORT
	THEN
    ELSE
	POSTPONE SLITERAL
	[ 'CROSS-COMPILER (') ] LITERAL [FORTH] CTCOMPILE,
    THEN ; IMMEDIATE

\ Compile the code needed to cross-compile at run-time the next
\ word in the input stream
: [COMPILE] \ ( "name" -- )
    PARSE-WORD
    [CROSS-COMPILER] CROSS-COMPILING? IF
	2DUP [CROSS-COMPILER] FIND IF
	    ROT 2DROP
	    S" (LITERAL)" [CROSS-COMPILER] (') [CROSS] CTCOMPILE,
	    [CROSS] XTCOMPILE,
	    S" CTCOMPILE," [CROSS-COMPILER] (') [CROSS] CTCOMPILE,
	ELSE
	    TYPE S" not found on target" ABORT
	THEN
    ELSE
	POSTPONE SLITERAL
	[ 'CROSS-COMPILER (') ] LITERAL [FORTH] CTCOMPILE,
	[ 'CROSS CTCOMPILE,   ] LITERAL [FORTH] CTCOMPILE,
    THEN ; IMMEDIATE


\ ---------- Literals ----------

\ Cross-compile the top of the stack as a literal
: LITERAL \ ( n -- )
    [CROSS-COMPILER] [COMPILE] (LITERAL)
    COMPILE, ; [FORTH] IMMEDIATE

\ Cross-compile the address on the top of the stack as a literal
: ALITERAL \ ( addr -- )
    [CROSS-COMPILER] [COMPILE] (LITERAL)
    ACOMPILE, ; [FORTH] IMMEDIATE

\ Cross-compile the xt on the top of the stack as a literal
: XTLITERAL \ ( xt -- )
    [CROSS-COMPILER] [COMPILE] (LITERAL)
    XTCOMPILE, ; [FORTH] IMMEDIATE

\ Cross-compile the cfa on the top of the stack as a literal
: CFALITERAL \ ( cfa -- )
    [CROSS-COMPILER] [COMPILE] (LITERAL)
    CFACOMPILE, ; [FORTH] IMMEDIATE

\ Cross-compile the string on the top of the stack as a literal
: SLITERAL \ ( addr len -- )
    [CROSS-COMPILER] [COMPILE] (SLITERAL)
    SCOMPILE, ; [FORTH] IMMEDIATE


\ ---------- The colon cross-compiler ----------

\ Look up a word in the cross-compiler
: CROSS-COMPILER-FIND ( addr n -- xt 1 | xt -1 | 0 )
    CROSS-COMPILER-WORDLIST SEARCH-WORDLIST ;

\ The cross-executive.
: CROSS-OUTER ( -- )
    BEGIN
	PARSE-WORD ?DUP IF
	    INTERPRETING? IF
		2DUP FIND IF
		    \ found, execute
		    ROT 2DROP
		    EXECUTE
		ELSE
		    \ no found, is it a number?
		    2DUP NUMBER? IF
			\ a number, leave on the stack
			ROT 2DROP
		    ELSE
			\ not a number, fail
			TYPE [FORTH] S" ?" ABORT
		    THEN
		THEN
	    ELSE
		[CROSS-COMPILER] CROSS-COMPILING? IF
		    2DUP [CROSS-COMPILER] FIND CASE
			1 OF
			    \ found, compile
			    ROT 2DROP
			    [CROSS] CTCOMPILE,
			ENDOF
			1 NEGATE OF
			    DROP
			    2DUP [CROSS-COMPILER] CROSS-COMPILER-FIND CASE
				1 OF
				    \ found but not immediate, fail
				    DROP
				    TYPE SPACE [FORTH] S" is shadowed but not IMMEDIATE on host" ABORT
				ENDOF
				1 NEGATE OF
				    ROT 2DROP
				    EXECUTE
				ENDOF
				0 OF
				    \ not found, fail
				    TYPE [FORTH] S" ?" ABORT
				ENDOF
			    ENDCASE
			ENDOF
			0 OF
			    2DUP [CROSS-COMPILER] CROSS-COMPILER-FIND CASE
				1 OF
				    \ found but not immediate, abort
				    DROP
				    TYPE SPACE [FORTH] S" is not a cross-compiler word" ABORT
				ENDOF
				1 NEGATE OF
				    \ found an immediate, execute
				    ROT 2DROP
				    EXECUTE
				ENDOF
				0 OF
				    \ not found, is it a number?
				    2DUP NUMBER? IF
					ROT 2DROP
					[ 'CROSS-COMPILER LITERAL [FORTH] CTCOMPILE, ]
				    ELSE
					\ not a number, fail
					TYPE [FORTH] S" ?" ABORT
				    THEN
				ENDOF
			    ENDCASE
			ENDOF
		    ENDCASE
		ELSE
		    2DUP FIND CASE
			1 OF
			    \ found, compile
			    ROT 2DROP
			    [FORTH] CTCOMPILE,
			ENDOF
			1 NEGATE OF
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
				TYPE [FORTH] S" ?" ABORT
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
	[ 'CROSS-COMPILER SLITERAL [FORTH] CTCOMPILE, ]
\    ELSE
\	S" String not delimited" ABORT
    THEN ; [FORTH] IMMEDIATE

\ Compile the next character as a literal
: [CHAR] \ ( "name" -- )
    PARSE-WORD CHAR [ 'CROSS-COMPILER LITERAL [FORTH] CTCOMPILE, ] ; [FORTH] IMMEDIATE

\ Postpone dynamically
: POSTPONE ( "name" -- )
    [CROSS-COMPILER] ' CTCOMPILE, ; [FORTH] IMMEDIATE


<WORDLISTS ONLY FORTH ALSO CROSS-COMPILER DEFINITIONS

\ Manipulate the compiler state of the host from the cross-compiler
: [ INTERPRETATION-STATE STATE ! ; IMMEDIATE
: ] COMPILATION-STATE    STATE ! ;

WORDLISTS>

\ Manage immediacy and redirect-ability of words on the target
: IMMEDIATE    [CROSS] IMMEDIATE-MASK    [CROSS] LASTXT [CROSS] SET-STATUS ; 
: REDIRECTABLE [CROSS] REDIRECTABLE-MASK [CROSS] LASTXT [CROSS] SET-STATUS ; 

\ Create a header and a locator
: (CROSS-WORD) ( addr n cf -- txt )
    >R 2DUP R> (HEADER,)
    CREATE-WORD-LOCATOR ;

'CROSS-COMPILER (CROSS-WORD) 'CROSS (WORD) (IS)

\ The colon-compiler
: COLON ( addr n -- txt )
    [CROSS-COMPILER] ['] (:) [CROSS] CFA@ [CROSS] (WORD)
    ] ;    
: : ( "name" -- txt )
    PARSE-WORD [CROSS-COMPILER] COLON ;
: :NONAME ( -- txt txt )
    NULLSTRING [CROSS-COMPILER] COLON DUP ;
: ; ( txt -- )
    NEXT,
    POSTPONE [
    DROP ; [FORTH] IMMEDIATE


<WORDLISTS ONLY FORTH ALSO CROSS-COMPILER DEFINITIONS

\ Manipulate the hidden status of the main colon-cross-compiler words. This allows
\ them to be hidden when including code into the cross-compiler itself, while
\ still letting that code use other cross-compiler words
: (COLON-CROSS-COMPILER) ( hider -- )
    [ 'CROSS-COMPILER :          ] LITERAL OVER EXECUTE
    [ 'CROSS-COMPILER ;          ] LITERAL OVER EXECUTE
    [ 'CROSS-COMPILER :NONAME    ] LITERAL OVER EXECUTE
    [ 'CROSS-COMPILER [          ] LITERAL OVER EXECUTE
    [ 'CROSS-COMPILER ]          ] LITERAL OVER EXECUTE
    [ 'CROSS-COMPILER POSTPONE   ] LITERAL OVER EXECUTE
    [ 'CROSS-COMPILER IMMEDIATE  ] LITERAL OVER EXECUTE
    [ 'CROSS          IMMEDIATE? ] LITERAL OVER EXECUTE
    DROP ;

\ Hide/unhide the core coon-cross-compiler
: HIDE-COLON-CROSS-COMPILER
    ['] (HIDE)   [CROSS-COMPILER] (COLON-CROSS-COMPILER) ;
: UNHIDE-COLON-CROSS-COMPILER
    ['] (UNHIDE) [CROSS-COMPILER] (COLON-CROSS-COMPILER) ;

WORDLISTS>
    
<WORDLISTS ONLY FORTH ALSO CROSS-COMPILER DEFINITIONS

\ Include using the host's include operations wrapped-up with a new executive
\ sd: not really safe....
: INCLUDED ( addr n -- )
    EXECUTIVE @ ROT
    [ 'CROSS-COMPILER CROSS-OUTER ] LITERAL EXECUTIVE !
    ." (include " 2DUP TYPE ." )" CR 
    INCLUDED
    EXECUTIVE ! ;
: INCLUDE ( "name" -- )
    PARSE-WORD [CROSS-COMPILER] INCLUDED ;

WORDLISTS>
