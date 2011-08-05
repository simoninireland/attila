\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
\ Copyright (c) 2007--2011, Simon Dobson <simon.dobson@computer.org>.
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

\ Tests of string handling
\
\ Since this is for automated testing, we only test the programmatic aspects
\ and not the I/O parts of the strings package.
\
\ We assume ASCII encoding, probably incorrectly

TESTCASES" String handling"

TESTING" Character encoding"

\ Test we can extract the first character correctly
{ CHAR hello -> 104 }


TESTING" Parsing"

{ PARSE-WORD hello NIP      -> 5 }
{ S" hello" NIP       -> 5 }
{ S" hello world" NIP -> 11 }


TESTING" String equality"

{ S" Hello"  S" Hello"  S=   -> TRUE }
{ S" Hello"  S" Hello " S=   -> FALSE }
{ S" Hello"  S" hello"  S=CI -> TRUE }
{ S" hello"  S" hello"  S=CI -> TRUE }


TESTING" Null string handling"

{ NULLSTRING NULLSTRING S=   -> TRUE } 
{ S" "       NULLSTRING S=   -> TRUE } 
{ NULLSTRING S" "       S=   -> TRUE }
{ S" 1"      NULLSTRING S=   -> FALSE }
{ NULLSTRING S" 1"      S=   -> FALSE }


TESTING" Counting"

{ HERE
  S" hello" S,
  COUNT NIP -> 5 }
{ HERE 0 C,
  COUNT NIP -> 0 }


TESTING" String data compilation"

{ HERE
  S" hello" S,
  99 C,                  \ check for overflows
  DUP            C@      \ count
  SWAP 1+    DUP C@      \ letters
  SWAP CHAR+ DUP C@
  SWAP CHAR+ DUP C@
  SWAP CHAR+ DUP C@
  SWAP CHAR+ DUP C@
  SWAP CHAR+ DUP C@ NIP -> 5 104 101 108 108 111 99 }


TESTING" String movement"

DATA STRING1 20 CHARS ALLOT

{ 99 STRING1 6 CHARS + C!
  S" hello" STRING1 SMOVE
  STRING1 C@
  STRING1 CHAR+ C@
  STRING1 6 CHARS + C@ -> 5 104 99 }

\ Empty string movement
{ 99 STRING1 1 CHARS + C!
  HERE 0 STRING1 SMOVE
  STRING1 C@
  STRING1 CHAR+ C@ -> 0 99 }
{ NULLSTRING STRING1 SMOVE
  STRING1 COUNT NIP -> 0 }
{ 99 STRING1 1 CHARS + C!
  HERE -10 STRING1 SMOVE
  STRING1 C@
  STRING1 CHAR+ C@ -> 0 99 }

\ Synonym
{ S" goodbye" STRING1 S!
 STRING1 COUNT S" goodbye" S= -> TRUE }


TESTING" String concatenation"

{ S" hello " STRING1 SMOVE
  STRING1 COUNT S" world" S+
  STRING1 C@
  STRING1 7 CHARS + C@ -> 11 119 }


TESTING" String reversing"

{ S" hello" STRING1 SMOVE
  STRING1 COUNT REVERSE
  STRING1    DUP C@      \ count
  SWAP 1+    DUP C@      \ letters
  SWAP CHAR+ DUP C@
  SWAP CHAR+ DUP C@
  SWAP CHAR+ DUP C@
  SWAP CHAR+ DUP C@ NIP -> 5 111 108 108 101 104 }

\ Empty and single-character strings
{ NULLSTRING STRING1 SMOVE
  STRING1 COUNT REVERSE 
  STRING1 COUNT NIP -> 0 }
{ S" h" STRING1 SMOVE
  STRING1 COUNT REVERSE
  STRING1 DUP C@
  SWAP 1+     C@ -> 1 104 }


TESTING" Indexing"

S" abhdefghij" STRING1 SMOVE

\ forward
{ STRING1 COUNT [CHAR] a INDEX -> 0 }
{ STRING1 COUNT [CHAR] b INDEX -> 1 }
{ STRING1 COUNT [CHAR] h INDEX -> 2 }
{ STRING1 COUNT [CHAR] j INDEX -> 9 }
{ STRING1 COUNT [CHAR] k INDEX -> -1 }

\ reverse
{ STRING1 COUNT [CHAR] a RINDEX -> 0 }
{ STRING1 COUNT [CHAR] b RINDEX -> 1 }
{ STRING1 COUNT [CHAR] h RINDEX -> 7 }
{ STRING1 COUNT [CHAR] j RINDEX -> 9 }
{ STRING1 COUNT [CHAR] k RINDEX -> -1 }

\ off-end checks
{ STRING1 COUNT 1-              [CHAR] j INDEX  -> -1 }
{ STRING1 COUNT 1- SWAP 1+ SWAP [CHAR] a INDEX  -> -1 }
{ STRING1 COUNT 1-              [CHAR] j RINDEX -> -1 }
{ STRING1 COUNT 1- SWAP 1+ SWAP [CHAR] a RINDEX -> -1 }


TESTING" Splitting"

\ lengths
{ S" ab:bc" [CHAR] : SPLIT -ROT 2DROP -ROT 2DROP -> 2 }
{ S" ab:bc:" [CHAR] : SPLIT -ROT 2DROP -ROT 2DROP -ROT 2DROP -> 3 }
{ S" ab:" [CHAR] : SPLIT -ROT 2DROP -ROT 2DROP -> 2 }
{ S" ab" [CHAR] : SPLIT -ROT 2DROP -> 1 }

\ \ extraction
{ S" ab" [CHAR] : SPLIT DROP S" ab" S= -> TRUE }
{ S" ab:" [CHAR] : SPLIT DROP S" ab" S= -ROT 2DROP -> TRUE }
{ S" ab:" [CHAR] : SPLIT DROP S" ab" S= -ROT NIP 0= -> TRUE TRUE }
{ S" ab:bc" [CHAR] : SPLIT DROP S" ab" S= -ROT S" bc" S= -> TRUE TRUE }


TESTING" Translating"

{ S" abhdefghij" STRING1 SMOVE
  STRING1 COUNT S" abh" S" ttt" TRANSLATE
  STRING1 COUNT S" tttdefgtij" S= -> TRUE }
{ S" abhdefghij" STRING1 SMOVE
  STRING1 COUNT S" zxr" S" ttt" TRANSLATE
  STRING1 COUNT S" abhdefghij" S= -> TRUE }
