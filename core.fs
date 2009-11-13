\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
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

\ Really core primitives that no system can do without

\ ---------- Nothing ----------

\ Do nothing, busily
PRIMITIVE: NOOP ( -- )
;PRIMITIVE


\ ---------- Stack manipulations ----------
\ sd: These should be more optimised

\ Duplicate the top stack item
PRIMITIVE: DUP ( a -- a a )
;PRIMITIVE

\ Duplicate the top stack item if it's non-zero
PRIMITIVE: ?DUP ( a -- )
    PUSH_CELL(a);
    if(a != 0) {
        PUSH_CELL(a);
    }
;PRIMITIVE

\ Drop the top stack item
PRIMITIVE: DROP ( a -- )
;PRIMITIVE

\ Remove the second stack item from under the first
PRIMITIVE: NIP ( a b -- b )
;PRIMITIVE

\ Swap the top two stack items
PRIMITIVE: SWAP ( a b -- b a )
;PRIMITIVE

\ Copy the second stack item to the top
PRIMITIVE: OVER ( a b -- a b a )
;PRIMITIVE

\ Rotate the top three stack items clockwise
PRIMITIVE: ROT ( a b c -- c a b )
;PRIMITIVE

\ Rotate the topthree stack items counter-clockwise
PRIMITIVE: -ROT ( a b c -- b c a )
;PRIMITIVE

\ Copy the n'th stack item to the top, counting from 0 not
\ including n, so 0 PICK is the same as DUP
PRIMITIVE: PICK ( n -- a )
    a = *(DATA_STACK_ITEM(n));
;PRIMITIVE

\ Put the depth of the stack onto the stack, not including the depth itself
PRIMITIVE: DEPTH ( -- d )
    d = DATA_STACK_DEPTH();
;PRIMITIVE


\ ---------- Literals ----------

\ Push the next cell in the instruction stream as a literal
PRIMITIVE: (LITERAL) ( -- l ) " bracket literal"
    CELLPTR addr = POP_RETURN();
    l = (*addr);
    PUSH_RETURN(addr + 1);
;PRIMITIVE

\ Push a string in the code space onto the stack as a standard
\ address-plus-count pair
PRIMITIVE: (SLITERAL) ( -- addr n ) " ess bracket literal"
    int len;
    addr = POP_RETURN();
    len = (*((BYTEPTR) addr));
    PUSH_RETURN(((BYTEPTR) addr) + 1 + len)); 
;PRIMITIVE


\ ---------- Constants ----------

\ False truth value
PRIMITIVE: FALSE ( -- f )
    f = 0;
;PRIMITIVE

\ True truth valuse, the ones-complement of FALSE
PRIMITIVE: TRUE ( -- f )
    f = !0;
;PRIMITIVE

\ The number of bytes needed to hold n cells
PRIMITIVE: CELLS ( n -- bs )
    bs = n * sizeof(CELL); 
;PRIMITIVE

\ The number of bytes needed to hold n characters, which are
\ typically - but not necessarily -- bytes
PRIMITIVE: CHARS ( n -- bs )
    bs = n * sizeof(CHARACTER); 
;PRIMITIVE


\ ---------- Basic arithmetic operations ----------

\ Single-precision addition
PRIMITIVE: + ( n1 n2 -- n3 )
    n3 = n1 + n2;
;PRIMITIVE

\ Single-precision subtraction
PRIMITIVE: - ( n1 n2 -- n3 )
    n3 = n1 - n2;
;PRIMITIVE

\ Single-precision multiplication
PRIMITIVE: * ( n1 n2 -- n3 )
    n3 = n1 * n2;
;PRIMITIVE

\ Single-precision integer quotient and remainder
PRIMITIVE: /MOD ( n1 n2 -- r q ) " slash mod"
    q = (CELL) (n1 / n2);
    r = (CELL) (n1 % n2);
;PRIMITIVE

\ Compute (n1*n2)/n3 and (n1*n2)%n3, where the multiplication
\ term is held in double precision"
PRIMITIVE: */MOD ( n1 n2 n3 -- r q ) " star slash mod"
    DOUBLE_CELL i = (DOUBLE_CELL) (n1 * n2);
    q = (CELL) (i / n3);
    r = (CELL) (i % n3);
;PRIMITIVE

\ Negate a number
PRIMITIVE: NEGATE ( n1 -- n2 )
    n2 = -n1;
;PRIMITIVE

\ Force number to be positive
PRIMITIVE: ABS ( n1 -- n2 )
    n2 = (n1 < 0) ? -n1 : n1;
;PRIMITIVE


\ ---------- Comparisons ----------
\ sd: This set isn't minimal, in that some could be defined in terms of
\ others. But there's no canonical way to decide which is which so we may
\ as well make them all equally fast

\ Extract the larger of two numbers
PRIMITIVE: MAX ( n1 n2 -- n3 )
    n3 = (n1 > n2) ? n1 : n2;
;PRIMITIVE

\Extract the smaller of two numbers
PRIMITIVE: MIN ( n1 n2 -- n3 )
    n3 = (n1 < n2) ? n1 : n2;
;PRIMITIVE

\ Test whether a is strictly greater than b
PRIMITIVE: > ( a b -- f ) " greater than"
    f = (a > b) ? TRUE : FALSE;
;PRIMITIVE

\ Test whether a is greater than or equal to b
PRIMITIVE: >= ( a b -- f ) " greater than or equal to"
    f = (a >= b) ? TRUE : FALSE;
;PRIMITIVE

\ Test whether a is strictly less than b
PRIMITIVE: < ( a b -- f ) " less than"
    f = (a < b) ? TRUE : FALSE;
;PRIMITIVE

\ Test whether a is less than or equal to b
PRIMITIVE: <= ( a b -- f ) " less than or equal to"
    f = (a <= b) ? TRUE : FALSE;
;PRIMITIVE

\ Test whether a and b are equal
PRIMITIVE: = ( a b -- f ) " equals"
    f = (a == b) ? TRUE : FALSE;
;PRIMITIVE

\ Test whether a is zero
PRIMITIVE: 0= ( a -- f ) " zero equals"
    f = (a == 0) ? TRUE : FALSE;
;PRIMITIVE

\ Test whether a is non-zero
PRIMITIVE: 0<> ( a -- f ) " zero not equals"
    f = (a != 0) ? TRUE : FALSE;
;PRIMITIVE

\ Test whether a is less than zero
PRIMITIVE: 0< ( a -- f ) " zero less than"
    f = (a < 0) ? TRUE : FALSE;
;PRIMITIVE

\ Test whether a is greater than zero
PRIMITIVE: 0> ( a -- f ) " zero greater than"
    f = (a > 0) ? TRUE : FALSE;
;PRIMITIVE


\ ---------- Bit-level operators ----------

\ Bitwise AND
PRIMITIVE: AND ( a b -- c )
    c = a & b;
;PRIMITIVE

\ Bitwise OR
PRIMITIVE: OR ( a b -- c )
    c = a | b;
;PRIMITIVE

\ Bitwise XOR
PRIMITIVE: XOR ( a b -- c )
    c = a ^ b;
;PRIMITIVE

\ Bitwise inversion
PRIMITIVE: INVERT ( a -- b )
    b  = ~a;
;PRIMITIVE

\ Shift a left n bits, replacing the rightmost n bits with zeros
PRIMITIVE: LSHIFT ( a n -- b ) " el shift"
    b = a << n
;PRIMITIVE

\ Shift a right n bits, replacing the leftmost n bits with zeros
PRIMITIVE: RSHIFT ( a n -- b ) " ar shift"
    b = a >> n
;PRIMITIVE


\ ---------- Common operations ----------
\ sd: For these to be worthwhile defining as primitives they need to be more optimised

\ Increment the top stack item    
PRIMITIVE: 1+ ( n1 -- n2 ) " one plus"
    n2 = n1 + 1;
;PRIMITIVE

\ Decrement the top stack item    
PRIMITIVE: 1- ( n1 -- n2 ) " one minus"
    n2 = n1 - 1;
;PRIMITIVE

\ Double the top stack item    
2* ( n1 -- n2 ) " two times"
    n2 = n1 << 1;
;PRIMITIVE

\ Halve the top stack item    
PRIMITIVE: 2/ ( n1 -- n2 ) " two divide"
    n2 = n1 >> 1;
;PRIMITIVE


\ ---------- Memory operations ----------

\ Store a value in a cell
PRIMITIVE: ! ( v addr -- ) " store"
    (*((CELLPTR) addr)) = (CELL) v;
;PRIMITIVE

\ Retrieve the cell at the given address
PRIMITIVE: @ ( addr -- v ) " fetch"
    v = (*((CELLPTR) addr));
;PRIMITIVE
   
\ Increment the value at the given address
PRIMITIVE: +! ( n adddr -- ) " plus store"
    (*((CELLPTR) addr)) += n;
;PRIMITIVE
    
\ Store a character at an address
PRIMITIVE: C! ( c addr -- ) " see store"
    (*((CHARACTERPTR) addr)) = (CHARACTER) c;
;PRIMITIVE

\ Retrieve the character at the given address
PRIMITIVE: C@ ( addr -- c ) " see fetch"
    c = (*((CHARACTERPTR) addr));
;PRIMITIVE

\ sd: These next two words are deliberately coded without using memcpy()
\ or bcopy() to minimise library dependencies    
    
\ Move a block upwards in memory, where addr1 < addr2
PRIMITIVE: CMOVE> ( addr1 addr2 n -- ) " see move up"
    for(int i = 0; i < n ; i++) {
        (*((BYTEPTR) addr2++) = (*((BYTEPTR) addr1++);
    }
;PRIMITIVE

\ Move a block downwards in memory, where addr2 < addr1
PRIMITIVE: CMOVE< ( addr1 addr2 n -- ) " see move down"
    for(int i = n - 1; i >= 0 ; i++) {
        (*((BYTEPTR) addr2++) = (*((BYTEPTR) addr1++);
    }
;PRIMITIVE
