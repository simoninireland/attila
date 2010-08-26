\ $Id$

\ Locators
\
\ Locators are word in TARGET that represent words in the target
\ image for the host. Each target word has a locator that can be
\ used to retrieve its xt and (for primitives) its name and C text.

\ ---------- Basic locator ----------

\ Locator words representing a word on the target
RECORD: LOCATOR
    1 CELLS FIELD: TARGET-XT            \ txt of word
;RECORD


\ ---------- Normal word locators ----------

\ Create a locator for the given target xt, leaving it on the stack
\ afterwards for convenience
: CREATE-WORD-LOCATOR \ ( addr n txt -- txt )
    ROT ['] LOCATOR ROT (CREATE-RECORD)
    DUP LASTXT EXECUTE TARGET-XT ! ;


\ ---------- Primitive locators ----------

\ Primitive locators
RECORD: PRIMITIVE-LOCATOR EXTENDS: LOCATOR
    1 CELLS FIELD: PRIMITIVE-NAME       \ Forth-level name
    1 CELLS FIELD: PRIMITIVE-PRIMNAME   \ underlying-level (typically C-level) name
    1 CELLS FIELD: PRIMITIVE-ARGUMENTS  \ list of argument names, as strings
    1 CELLS FIELD: PRIMITIVE-RESULTS    \ list of result names, as strings
    1 CELLS FIELD: PRIMITIVE-TEXT       \ zstring of text
;RECORD

\ Return the CFA, which is the counted-string address of the primname
: PRIMITIVE-CFA PRIMITIVE-PRIMNAME ;

\ Create a new primitive locator (leaves txt unset)
: CREATE-PRIMITIVE-LOCATOR ( naddr paddr al rl zaddr -- )
    ['] PRIMITIVE-LOCATOR 5 PICK COUNT (CREATE-RECORD)
    LASTXT EXECUTE >R
    R@ PRIMITIVE-TEXT      A!
    R@ PRIMITIVE-RESULTS   A!
    R@ PRIMITIVE-ARGUMENTS A!
    R@ PRIMITIVE-PRIMNAME  A!
    R> PRIMITIVE-NAME      A! ;
    
