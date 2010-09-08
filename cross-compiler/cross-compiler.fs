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

\ The Attila cross-compiler


\ ---------- More precise word list handling ----------

\ Find a word in a given wordlist, aborting if we fail
: FIND-IN-WORDLIST \ ( addr n wid -- xt )
    >R 2DUP R> SEARCH-WORDLIST
    0<> IF
	ROT 2DROP
    ELSE
	TYPE SPACE
	S" not found in word list" ABORT
    THEN ;

\ Immediate look-up in a specific wordlist
: ['WORDLIST] \ ( wid "name" -- xt )
    PARSE-WORD -ROT FIND-IN-WORDLIST ; IMMEDIATE

\ Look-up and execute/compile a word from (only) the given word list
\ sd: This may be a common pattern: should it be moved into wordlists.fs?
:NONAME \ ( wid "name" -- )
    POSTPONE ['WORDLIST]
    EXECUTE ;
:NONAME \ ( wid "name" -- )
    POSTPONE ['WORDLIST]
    DUP IMMEDIATE? IF
	EXECUTE     \ execute any immediate words as normal
    ELSE
	CTCOMPILE,  \ compile normal words
    THEN ;
INTERPRET/COMPILE [WORDLIST]

    
\ ---------- The word lists ----------

\ Placed in ROOT so that we can access them even when we're not
\ searching FORTH
<WORDLISTS ALSO ROOT DEFINITIONS

NAMED-WORDLIST TARGET             CONSTANT TARGET-WORDLIST              \ locators
NAMED-WORDLIST CROSS              CONSTANT CROSS-WORDLIST               \ image manipulation
NAMED-WORDLIST CROSS-COMPILER     CONSTANT CROSS-COMPILER-WORDLIST      \ cross-compiler IMMEDIATE words
NAMED-WORDLIST CODE-GENERATOR     CONSTANT CODE-GENERATOR-WORDLIST      \ code generator

TARGET-WORDLIST             PARSE-WORD TARGET             (VOCABULARY)
CROSS-WORDLIST              PARSE-WORD CROSS              (VOCABULARY)
CROSS-COMPILER-WORDLIST     PARSE-WORD CROSS-COMPILER     (VOCABULARY)
CODE-GENERATOR-WORDLIST     PARSE-WORD CODE-GENERATOR     (VOCABULARY)

\ Look-up and execute/compile the next word from the given wordlist
: [TARGET]             TARGET-WORDLIST              POSTPONE [WORDLIST]  ; IMMEDIATE
: [CROSS]              CROSS-WORDLIST               POSTPONE [WORDLIST]  ; IMMEDIATE
: [CROSS-COMPILER]     CROSS-COMPILER-WORDLIST      POSTPONE [WORDLIST]  ; IMMEDIATE
: [CODE-GENERATOR]     CODE-GENERATOR-WORDLIST      POSTPONE [WORDLIST]  ; IMMEDIATE
: [FORTH]              FORTH-WORDLIST               POSTPONE [WORDLIST]  ; IMMEDIATE

\ Look-up the next word in the input stream from the given wordlist
: 'TARGET              TARGET-WORDLIST              POSTPONE ['WORDLIST] ;
: 'CROSS               CROSS-WORDLIST               POSTPONE ['WORDLIST] ;
: 'CROSS-COMPILER      CROSS-COMPILER-WORDLIST      POSTPONE ['WORDLIST] ;
: 'FORTH               FORTH-WORDLIST               POSTPONE ['WORDLIST] ;

\ Display the key wordlists alone, for debugging
: TARGET-WORDS             TARGET-WORDLIST             .WORDLIST ;
: CROSS-WORDS              CROSS-WORDLIST              .WORDLIST ;
: CROSS-COMPILER-WORDS     CROSS-COMPILER-WORDLIST     .WORDLIST ;
: CODE-GENERATOR-WORDS     CODE-GENERATOR-WORDLIST     .WORDLIST ;
WORDLISTS>


\ ---------- The main body of the cross-compiler ----------

<WORDLISTS ONLY FORTH ALSO CODE-GENERATOR ALSO DEFINITIONS
\ Current version string
: VERSION S" v1 $Date$" ;

\ Generate a timestamp string
: TIMESTAMP
    ." Attila cross-compiler " VERSION TYPE ;
WORDLISTS>

<WORDLISTS ONLY FORTH ALSO CROSS ALSO DEFINITIONS
\ Pre-defining unavoidable forward references
DEFER USERVAR
DEFER (HEADER,)
DEFER (WORD)
DEFER CTCOMPILE,
DEFER NEXT,

.( Loading target vm description...)
: CONSTANT
    CREATE [FORTH] ,
  DOES> [FORTH] @ ;
: SCONSTANT
    CREATE [FORTH] S,
  DOES> [FORTH] COUNT ;
: USER
    CREATE [FORTH] ,
  DOES> [FORTH] @ [CROSS] USERVAR ;

include vm.fs
WORDLISTS>

.( Loading target character handlers...)
<WORDLISTS ONLY FORTH ALSO CROSS ALSO DEFINITIONS
include ascii.fs
WORDLISTS>

.( Loading image manager...)
<WORDLISTS ONLY FORTH ALSO CROSS ALSO DEFINITIONS
include cross-compiler/cross-image-fixedsize.fs    \ sd: should come from elsewhere
WORDLISTS>

.( Loading locator structures...)
<WORDLISTS ONLY FORTH ALSO DEFINITIONS
include cross-compiler/cross-locator.fs
WORDLISTS>

.( Loading cross-compiler memory model...)
<WORDLISTS ONLY FORTH ALSO CROSS ALSO DEFINITIONS
include cross-compiler/cross-flat-memory-model.fs
WORDLISTS>

.( Loading cross-compiler interpretation model...)
<WORDLISTS ONLY FORTH ALSO CROSS ALSO DEFINITIONS
include itil.fs
WORDLISTS>

<WORDLISTS ONLY FORTH ALSO CROSS ALSO DEFINITIONS
: IMMEDIATE?    GET-STATUS IMMEDIATE-MASK    AND 0<> ;
: REDIRECTABLE? GET-STATUS REDIRECTABLE-MASK AND 0<> ;
WORDLISTS>

.( Loading C language primitive compiler...)
<WORDLISTS ONLY FORTH ALSO DEFINITIONS
include cross-compiler/cross-c.fs
WORDLISTS>

.( Loading vm description generator...)
<WORDLISTS ONLY FORTH ALSO CODE-GENERATOR ALSO DEFINITIONS
include cross-compiler/cross-c-vm.fs
WORDLISTS>

.( Loading cross-compiler file generators... )
<WORDLISTS ONLY FORTH ALSO CROSS ALSO CODE-GENERATOR ALSO DEFINITIONS
include cross-compiler/cross-c-generate.fs
WORDLISTS>


.( Loading colon cross-compiler...)
<WORDLISTS ONLY FORTH ALSO CROSS ALSO CROSS-COMPILER DEFINITIONS
include cross-compiler/cross-colon.fs

: CONSTANT
    CREATE IMMEDIATE [FORTH] ,
  DOES> [FORTH] @
    INTERPRETING? NOT IF
	[CROSS-COMPILER] CROSS-COMPILING? IF
	    [ 'CROSS-COMPILER LITERAL [FORTH] CTCOMPILE, ]
	ELSE
	    POSTPONE LITERAL
	THEN
    THEN ;
: SCONSTANT
    CREATE IMMEDIATE [FORTH] S,
  DOES> [FORTH] COUNT
    INTERPRETING? NOT IF
	[CROSS-COMPILER] CROSS-COMPILING? IF
	    [ 'CROSS-COMPILER SLITERAL [FORTH] CTCOMPILE, ]
	ELSE
	    POSTPONE SLITERAL
	THEN
    THEN ;
: USER
    CREATE IMMEDIATE [FORTH] ,
  DOES> [FORTH] @ [CROSS] USERVAR
    INTERPRETING? NOT IF
	[CROSS-COMPILER] CROSS-COMPILING? IF
	    [ 'CROSS-COMPILER ALITERAL [FORTH] CTCOMPILE, ]
	ELSE
	    POSTPONE LITERAL
	THEN
    THEN ;
WORDLISTS>

.( Loading cross-compiler coding environments...)
<WORDLISTS ONLY FORTH ALSO ROOT DEFINITIONS
include cross-compiler/cross-environments.fs
WORDLISTS>

.( Loading cross-compiler comment handling...)
<WORDLISTS ONLY FORTH ALSO CROSS-COMPILER DEFINITIONS

include comments.fs
WORDLISTS>

.( Cross-compiler loaded)
