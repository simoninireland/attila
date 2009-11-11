\ $Id: defer.fs,v 1.5 2007/06/09 12:15:50 sd Exp $

\ This file is part of Attila, a minimal threaded interpretive language
\ Copyright (c) 2007, UCD Dublin. All rights reserved.
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
    ['] (DEFERRED) ,
  DOES>
    @ EXECUTE ;

\ Re-map the behaviour of a deferred word
: IS \ ( xt "name" -- )
    PARSE-WORD FIND
    [ ' (?BRANCH) COMPILE, TOP 0 COMPILE, ]                        \ IF
	>BODY !
    [ ' (BRANCH) COMPILE, TOP 0 COMPILE, SWAP DUP JUMP> SWAP ! ]   \ ELSE
	S" Word not found" ABORT
    [ DUP JUMP> SWAP ! ] ;                                         \ THEN

	