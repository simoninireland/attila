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

\ Deferred words
\
\ A deferred word appears in the word list and so can be used, but
\ by default does not have a real behaviour. It can then be re-mapped
\ later to execute another word.
\ Requires createdoes.fs

\ The default behaviour for a deferred word
: (DEFERRED) \ ( -- )
    S" Deferred" ABORT ;

\ Create a deferred word
: DEFER \ ( "name" -- )
    CREATE
    ['] (DEFERRED) XT,
  DOES>
    @ EXECUTE ;

\ Re-map a deferred word
: (IS) \ ( xt dxt -- )
    >BODY XT! ;

\ Re-map a deferred word from the input stream
: IS \ ( xt "name" -- )
    PARSE-WORD 2DUP FIND IF
	-ROT 2DROP (IS)
    ELSE
	TYPE S" ?" ABORT
    THEN ;

