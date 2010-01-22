\ $Id$

\ The Attila cross-compiler

\ ---------- A new state ----------

2 CONSTANT CROSS-COMPILATION-STATE

\ Test whether we are cross-compiling
: CROSS-COMPILING? STATE @ CROSS-COMPILATION-STATE = ;


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

.( Cross-compiler wordlists...)
NAMED-WORDLIST CROSS          CONSTANT CROSS-WORDLIST
NAMED-WORDLIST TARGET         CONSTANT TARGET-WORDLIST
NAMED-WORDLIST CROSS-COMPILER CONSTANT CROSS-COMPILER-WORDLIST
NAMED-WORDLIST CODE-GENERATOR CONSTANT CODE-GENERATOR-WORDLIST

CROSS-WORDLIST          PARSE-WORD CROSS          (VOCABULARY)
TARGET-WORDLIST         PARSE-WORD TARGET         (VOCABULARY)
CROSS-COMPILER-WORDLIST PARSE-WORD CROSS-COMPILER (VOCABULARY)
CODE-GENERATOR-WORDLIST PARSE-WORD CODE-GENERATOR (VOCABULARY)

\ Look-up and execute/compile the next word from the given wordlist
: [CROSS]          CROSS-WORDLIST           POSTPONE [WORDLIST]  ; IMMEDIATE
: [TARGET]         TARGET-WORDLIST          POSTPONE [WORDLIST]  ; IMMEDIATE
: [CROSS-COMPILER] CROSS-COMPILER-WORDLIST  POSTPONE [WORDLIST]  ; IMMEDIATE
: [FORTH]          FORTH-WORDLIST           POSTPONE [WORDLIST]  ; IMMEDIATE
: [CODE-GENERATOR] CODE-GENERATOR-WORDLIST  POSTPONE [WORDLIST]  ; IMMEDIATE

\ Look-up the next word in the input stream from the given wordlist
: 'CROSS           CROSS-WORDLIST           POSTPONE ['WORDLIST] ;
: 'TARGET          TARGET-WORDLIST          POSTPONE ['WORDLIST] ;
: 'CROSS-COMPILER  CROSS-COMPILER-WORDLIST  POSTPONE ['WORDLIST] ;
: 'FORTH           FORTH-WORDLIST           POSTPONE ['WORDLIST] ;

\ Display the key wordlists alone, for debugging
: TARGET-WORDS         TARGET-WORDLIST         .WORDLIST ;
: CROSS-WORDS          CROSS-WORDLIST          .WORDLIST ;
: CROSS-COMPILER-WORDS CROSS-COMPILER-WORDLIST .WORDLIST ;
: CODE-GENERATOR-WORDS CODE-GENERATOR-WORDLIST .WORDLIST ;

WORDLISTS>


\ ---------- The main body of the cross-compiler ----------

<WORDLISTS ALSO CODE-GENERATOR ALSO DEFINITIONS

\ Current version string
: VERSION S" v0.2 alpha $Date$" ;

\ Generate a timestamp string
: TIMESTAMP
    ." Attila cross-compiler " VERSION TYPE ;

WORDLISTS>

<WORDLISTS ALSO CROSS ALSO DEFINITIONS

\ Prepare for loading files with comments in them
include comments.fs

\ Target is ASCII-based
include ascii.fs

\ Pre-defining unavoidable forward references
DEFER >CFA
DEFER (HEADER,)
DEFER CTCOMPILE,
DEFER NEXT,

\ Sizes of architectural elements, and other characteristics
4 VALUE /CELL
4 VALUE /CHARACTER
0 VALUE BIGENDIAN?
: CELLS \ ( n -- bs )
    /CELL * ;

.( Loading image manager...)
include x-image-gcc.fs    \ sd: should come from elsewhere

WORDLISTS>

.( Loading locator structures...)
include cross-locator.fs
.( Loading cross-compiler memory model...)
include cross-flat-memory-model.fs
.( Loading C language primitive compiler...)
include cross-c.fs
.( Loading colon cross-compiler...)
include cross-colon.fs
.( Loading cross create-does...)
include cross-createdoes.fs
.( Loading vm description generator...)
include cross-vm.fs
.( Loading cross-compiler file generators...)
include cross-generate.fs
.( Loading cross-compiler coding environments...)
include cross-environments.fs

.( Cross-compiler loaded)


