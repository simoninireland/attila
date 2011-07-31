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

\ CREATE ... DOES>
\
\ This relies on redirectable words having a virtual header field added
\ at the start of their bodies to store the indirect body address. It also
\ relies on >BODY understanding the redirectable field

\ Create a data block that returns its body address when executed
: (DATA) \ ( addr n -- )
    START-DEFINITION
    ['] (VAR) CFA@ (WORD)
    END-DEFINITION
    DROP ALIGNED ;
    
\ Create a data block from the next word in the input
: DATA \ ( "name" -- )
    PARSE-WORD (DATA) ;

\ Create a data block that can have its run-time behaviour re-directed
: (CREATE) \ ( addr n -- )
    (DATA)
    1 CELLS ALLOT           \ the indirect body address (iba)
    REDIRECTABLE ;          \ fix status
    
\ Create a data block from the next word in the input
: CREATE \ ( "name" -- )
    PARSE-WORD (CREATE) ;

\ Re-direct the run-time behaviour of a word which must be created
\ using (CREATE) or CREATE to make it redirectable
: (DOES>) \ ( xt -- )
    ['] (DOES) CFA@  OVER CFA!   \ point defined word's cfa to (DOES)
    R> SWAP IBA! ;               \ store next cell in defined word's iba,
                                 \ and then return without executing
                                 \ that code during the defining word

\ Compile the (DOES>) behaviour, leaving the newly-defined word's xt on the
\ stack for it to use in re-directing that word's run-time behaviour
: DOES> \ ( -- )
    POSTPONE LASTXT
    POSTPONE (DOES>) ; IMMEDIATE
