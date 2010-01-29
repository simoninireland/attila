\ $Id$

\ Constants
\
\ Constants return their value, substituting it literally if compiling.

\ Create a constant, which substitutes its value immediately
\ as a literal
: CONSTANT \ ( v "name" -- )
    CREATE IMMEDIATE ,
  DOES> @
    INTERPRETING? NOT IF
        POSTPONE LITERAL
    THEN ;
