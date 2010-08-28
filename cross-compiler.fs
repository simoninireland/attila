\ $Id$

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

    
\ ---------- Helpers ----------

\ Placeholder for C-level inclusions from architecture description
: CINCLUDE \ ( "name" -- )
    PARSE-WORD 2DROP ;


\ ---------- The word lists ----------

\ Placed in ROOT so that we can access them even when we're not
\ searching FORTH
<WORDLISTS ALSO ROOT DEFINITIONS

.( Creating cross-compiler wordlists...)
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

.( Loading cross-compiler coding environments...)
include cross-environments.fs

WORDLISTS>


\ ---------- The main body of the cross-compiler ----------

<CODE-GENERATOR

\ Current version string
: VERSION S" v0.2 alpha $Date$" ;

\ Generate a timestamp string
: TIMESTAMP
    ." Attila cross-compiler " VERSION TYPE ;

CODE-GENERATOR>

<CROSS

\ Target is ASCII-based
\ include ascii.fs

\ Pre-defining unavoidable forward references
DEFER >CFA
DEFER (HEADER,)
DEFER CTCOMPILE,
DEFER NEXT,

\ Sizes of architectural elements, and other characteristics. These are
\ defaults that can be overridden later
4 VALUE /CELL
1 VALUE /CHAR
1 VALUE BIGENDIAN?
: CELLS \ ( n -- bs )
    /CELL * ;

.( Loading image manager...)
include c-image-fixedsize.fs    \ sd: should come from elsewhere

.( Loading target vm description...)
<wordlists only forth also cross definitions
: USER
    CREATE IMMEDIATE , ." create user variable" cr
  DOES> @ [CROSS] USERVAR ." USERVAR " dup . cr ;
wordlists>
<wordlists only forth also cross also definitions
include vm.fs
wordlists>

.( Loading locator structures...)
<WORDLISTS ONLY FORTH ALSO DEFINITIONS
include cross-locator.fs
WORDLISTS>
.( Loading cross-compiler memory model...)
include cross-flat-memory-model.fs
<wordlists only forth also cross also definitions
: IMMEDIATE?    GET-STATUS IMMEDIATE-MASK    AND 0<> ;
: REDIRECTABLE? GET-STATUS REDIRECTABLE-MASK AND 0<> ;
wordlists>
.( Loading C language primitive compiler...)
<WORDLISTS ONLY FORTH ALSO DEFINITIONS
include cross-c.fs
WORDLISTS>
.( Loading cross create-does...)
\ include cross-createdoes.fs
<CODE-GENERATOR
.( Loading vm description generator...)
include cross-vm.fs
.( Loading cross-compiler file generators... )
include cross-generate.fs
CODE-GENERATOR>
.( Loading colon cross-compiler...)
<CROSS-COMPILER
include cross-colon.fs
CROSS-COMPILER>

CROSS>

.( Cross-compiler loaded)


