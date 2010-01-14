\ $Id$

\ Values
\
\ Values are a variant of constants whose value can be updated.

\ Create a value, which returns its value when executed
: VALUE \ ( v "name" -- )
    CREATE ,
  DOES> @ ;

\ Re-assign a value
' !
:NONAME POSTPONE LITERAL [COMPILE] ! ;
INTERPRET/COMPILE (TO)

\ Top-level TO
: TO \ ( v "name" -- )
    ' >BODY POSTPONE (TO) ; IMMEDIATE
