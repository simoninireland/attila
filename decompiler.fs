\ $Id$

\ Simple decompiler
\
\ There are several things wrong with this implementation. Firstly,
\ there is no indication of which control structure gives rise to which (BRANCH)
\ etc, and no indication of nesting. This would need some help placed into the
\ code. Secondly, we assume an indirect-threaded model, since we assume a body is
\ composed of xts and not cts. We should perhaps allow for a reversal from the
\ latter to the former -- a word like CT>XT perhaps.
\
\ However flawed, though, the current implementation helps debugging significantly.


\ ---------- Indentation and raw output ----------

\ Current indentation
VARIABLE DECOMPILER-LINE-INDENTATION

\ Line indentation increment -- the "standard" value for Forth code layout
4 VALUE DECOMPILER-LINE-INDENTATION-INCREMENT

\ Indent the line appropriately
: DECOMPILER-INDENT-LINE ( -- )
    DECOMPILER-LINE-INDENTATION @ SPACES ;

\ Indent and de-indent the current display
: DECOMPILER-INDENT   DECOMPILER-LINE-INDENTATION-INCREMENT        DECOMPILER-LINE-INDENTATION +! ;
: DECOMPILER-DEINDENT DECOMPILER-LINE-INDENTATION-INCREMENT NEGATE DECOMPILER-LINE-INDENTATION +! ;


\ ---------- Special words ----------

\ Maximum number of special words -- should be enough
32 VALUE MAX-SPECIAL-WORDS

\ Look-up table for "special" words that hack the compiled space, for example
\ by compiling literals and strings. Each entry is an xt of the special word
\ coupled with the xt of the display word for it
DATA SPECIAL-WORDS-TABLE MAX-SPECIAL-WORDS 2* CELLS ALLOT

\ Current number of special words
0 VALUE SPECIAL-WORDS

\ Add a special word to the table
: ADD-SPECIAL-WORD ( display-xt xt -- )
    SPECIAL-WORDS 2* CELLS SPECIAL-WORDS-TABLE +
    DUP ROT ! 1 CELLS + !
    SPECIAL-WORDS 1+ TO SPECIAL-WORDS ;

\ Each special word should have a stack effect
\    ( ip-before xt -- ip-after )
\ and display the given word and leave the ip ready for the next word,
\ accounting for any stack mischief on the part of the special word

\ Display a literal as itself
:NONAME
    DROP
    1 CELLS +
    DUP @ . SPACE
    1 CELLS + ;
' (LITERAL) ADD-SPECIAL-WORD

\ Display a string as S" abc"
:NONAME
    DROP
    1 CELLS +
    DUP [CHAR] S EMIT [CHAR] " EMIT SPACE COUNT TYPE [CHAR] " EMIT SPACE
    DUP C@ 1+ CHARS + CALIGN ;
' (SLITERAL) ADD-SPECIAL-WORD

\ Display branches as offsets
:NONAME
    DROP
    1 CELLS +
    ." (BRANCH) " DUP @ . CR
    1 CELLS + ;
' (BRANCH) ADD-SPECIAL-WORD
:NONAME
    DROP
    1 CELLS +
    ." (?BRANCH) " DUP @ . CR
    1 CELLS + ;
' (?BRANCH) ADD-SPECIAL-WORD

\ Display a "normal" (non-special) word
: (DISPLAY-NONSPECIAL-WORD) ( ip xt -- ip' )
    \ >NAME TYPE SPACE
    .HEX SPACE
    1 CELLS + ;

\ Look-up a word in the special words table, returning its display word or 0
: SPECIAL-WORD? ( xt -- display-xt | 0 )
    0 SPECIAL-WORDS-TABLE
    SPECIAL-WORDS 0 DO
	DUP @                ( xt 0 swt sxt )
	3 PICK = IF          ( xt 0 swt )
	    DUP 1 CELLS + @  ( xt 0 swt dxt )
	    -ROT DROP SWAP   ( xt dst swt )
	    LEAVE
	ELSE                 ( xt 0 swt )
	    2 CELLS +
	THEN
    LOOP
    DROP NIP ;
	
\ Return the word used to display the given word. This will be from the special
\ word table, or (DISPLAY-NONSPECIAL-WORD) for non-special words
: (DISPLAY-WORD) ( xt -- display-xt )
    SPECIAL-WORD? ?DUP 0= IF
	['] (DISPLAY-NONSPECIAL-WORD)
    THEN ;


\ ---------- Decompilation helpers ----------

\ Show a single word, returning the next instruction pointer for the next word
: SHOW-WORD ( ip xt -- ip' )
    DUP (DISPLAY-WORD) EXECUTE ;

\ Decompile a colon-definition
: (DECOMPILE-COLON-DEFINITION) ( xt -- )
    >BODY
    BEGIN
	DUP @ ?DUP
    WHILE
	    SHOW-WORD
    REPEAT
    DROP ;

\ Decompile a data block, just printing that it is indeed data
: (DECOMPILE-DATA) ( xt -- )
    ." <<data at " >BODY . ." >>" CR ;

\ Decompile a CREATE..>DOES> word. Not really adequate
: (DECOMPILE-CREATEDOES) ( xt -- )
    DROP ." CREATE...DOES>" CR ;

\ Decompile a primitive, which simply prints its cfa
: (DECOMPILE-PRIMITIVE) ( xt -- )
    ." <<primitive " CFA@ . ." >>" CR ;


\ ---------- Top-level decompilation words ----------

\ Decompile a word, selecting the appropriate decompiler
: (DECOMPILE) ( xt -- )
    0 DECOMPILER-LINE-INDENTATION !
    DUP REDIRECTABLE? IF
	\ CREATE...DOES> word
	(DECOMPILE-CREATEDOES)
    ELSE
	DUP CFA@ CASE
	    ['] (:) CFA@ OF
		." : " DUP >NAME TYPE CR
		DECOMPILER-INDENT
		(DECOMPILE-COLON-DEFINITION)
		DECOMPILER-DEINDENT
		." ; " CR
	    ENDOF
	    ['] (VAR) OF
		(DECOMPILE-DATA)
	    ENDOF
	    DUP OF
		(DECOMPILE-PRIMITIVE)
	    ENDOF
	ENDCASE
    THEN ;

\ Decompile the next word from the input stream
: DECOMPILE ( "name" -- )
    ' (DECOMPILE) ;

		
		    