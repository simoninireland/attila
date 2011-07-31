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
    -ROT ['] LOCATOR -ROT (CREATE-RECORD)
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
    
