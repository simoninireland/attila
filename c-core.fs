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

\ Really core primitives that no system can do without

\ ---------- Nothing ----------

\ Do nothing, busily
C: NOOP ( -- )
;C


\ ---------- Stack manipulations ----------
\ sd: These should be more optimised

\ Duplicate the top stack item
C: DUP ( a -- a a )
;C

\ Duplicate the top stack item if it's non-zero
C: ?DUP ( a -- )
    PUSH_CELL(a);
    if(a != 0) {
        PUSH_CELL(a);
    }
;C

\ Drop the top stack item
C: DROP ( a -- )
;C

\ Remove the second stack item from under the first
C: NIP ( a b -- b )
;C

\ Tuck the top element below the second
C: TUCK ( a b -- b a b )
;C

\ Swap the top two stack items
C: SWAP ( a b -- b a )
;C

\ Copy the second stack item to the top
C: OVER ( a b -- a b a )
;C

\ Rotate the top three stack items clockwise
C: ROT ( a b c -- c a b )
;C

\ Rotate the top three stack items counter-clockwise
C: -ROT ( a b c -- b c a )
;C

\ Roll the stack clockwise. 0 ROLL is a no-op; 1 ROLL is SWAP;
\ 2 ROLL is ROT
C: ROLL ( n -- )
  int t;
  int i;

  t = *(DATA_STACK_ITEM(0));
  for(i = 0; i < n; i++)
    *(DATA_STACK_ITEM(i)) = *(DATA_STACK_ITEM(i + 1));
  *(DATA_STACK_ITEM(n)) = t;
;C

\ Roll the stack counter-clockwise. 0 ROLL is a no-op; 1 ROLL is SWAP;
\ 2 ROLL is -ROT
C: -ROLL ( n -- )
  int b;
  int i;

  b = *(DATA_STACK_ITEM(n));
  for(i = n; i > 0; i--) 
    *(DATA_STACK_ITEM(i)) = *(DATA_STACK_ITEM(i - 1));
  *(DATA_STACK_ITEM(0)) = b;
;C

\ Copy the n'th stack item to the top, counting from 0 not
\ including n, so 0 PICK is the same as DUP, 1 PICK is OVER
C: PICK ( n -- a )
    a = *(DATA_STACK_ITEM(n));
;C

\ Put the depth of the stack onto the stack, not including the depth itself
C: DEPTH ( -- d )
    d = DATA_STACK_DEPTH();
;C

\ Drop the top two stack items
C: 2DROP ( a b -- )
;C

\ Duplicate the top two stack items
C: 2DUP ( a b -- a b a b )
;C

\ Swap the top two double-cells
C: 2SWAP ( a b c d -- c d a b )
;C

\ Copy the second double-cell to the top
C: 2OVER ( a b c d -- a b c d a b )
;C

\ Move the top item on the return stack to the data stack
C: R> ( -- addr )
    addr = (CELL) POP_RETURN();
;C

\ Drop the top return stack item
C: RDROP ( -- )
    POP_RETURN();
;C

\ Copy the top item on the return stack to the data stack, without
\ altering the return stack
C: R@ ( -- addr )
    addr = (CELL) PEEK_RETURN();
;C

\ Move the top item on the data stack to the return stack
C: >R ( addr -- )
    PUSH_RETURN((XT) addr);
;C

\ Copy the n'th return stack item to the data stack counting from 0,
\ so 0 PICK is the same as R>
C: RPICK ( n -- addr )
    addr = (CELL) *(RETURN_STACK_ITEM(n));
;C

\ Stash a number of items on the return stack
C: N>R ( n -- )
    int i;
    CELL v;

    for(i = 0; i < n; i++) {
         v = POP_CELL();
         PUSH_RETURN((XT) v);
    }
    PUSH_RETURN((CELL) n);
;C

\ De-stash the last stash
C: NR> ( -- n )
    int i;
    CELL v;

    n = (CELL) POP_RETURN();
    for(i = 0; i < n; i++) {
        v = (CELL) POP_RETURN();
        PUSH_CELL(v);
    }
;C


\ ---------- Constants ----------

\ The number of bytes per cell on this architecture
C: /CELL ( -- bs )
    bs = sizeof(CELL);
;C

\ The number of bytes per character on this architecture
C: /CHAR ( -- bs )
    bs = sizeof(CHARACTER);
;C


\ ---------- Basic arithmetic operations ----------

\ Single-precision addition
C: + ( n1 n2 -- n3 )
    n3 = n1 + n2;
;C

\ Single-precision subtraction
C: - ( n1 n2 -- n3 )
    n3 = n1 - n2;
;C

\ Single-precision multiplication
C: * ( n1 n2 -- n3 )
    n3 = n1 * n2;
;C

\ Single-precision integer quotient and remainder
C: /MOD ( n1 n2 -- r q )
    q = (CELL) (n1 / n2);
    r = (CELL) (n1 % n2);
;C

\ Compute (n1*n2)/n3 and (n1*n2)%n3, where the multiplication
\ term is held in double precision"
C: */MOD ( n1 n2 n3 -- r q )
    DOUBLE_CELL i;

    i = (DOUBLE_CELL) (n1 * n2);
    q = (CELL) (i / (DOUBLE_CELL) n3);
    r = (CELL) (i % (DOUBLE_CELL) n3);
;C

\ Negate a number
C: NEGATE ( n1 -- n2 )
    n2 = -n1;
;C

\ Force number to be positive
C: ABS ( n1 -- n2 )
    n2 = (n1 < 0) ? -n1 : n1;
;C


\ ---------- Comparisons ----------
\ sd: This set isn't minimal, in that some could be defined in terms of
\ others. But there's no canonical way to decide which is which so we may
\ as well make them all equally fast

\ Extract the larger of two numbers
C: MAX ( n1 n2 -- n3 )
    n3 = (n1 > n2) ? n1 : n2;
;C

\ Extract the smaller of two numbers
C: MIN ( n1 n2 -- n3 )
    n3 = (n1 < n2) ? n1 : n2;
;C

\ Test whether a is strictly greater than b
C: > ( a b -- f )
    f = (a > b) ? TRUE : FALSE;
;C

\ Test whether a is greater than or equal to b
C: >= ( a b -- f )
    f = (a >= b) ? TRUE : FALSE;
;C

\ Test whether a is strictly less than b
C: < ( a b -- f )
    f = (a < b) ? TRUE : FALSE;
;C

\ Test whether a lies in the range of [b..c]
C: WITHIN ( a b c -- f )
   f = (((b < c) && ((b <= a) && (a < c))) ||
        ((b > c) && ((b <= a) || (a < c)))) ? TRUE : FALSE ;
;C
	
\ Test whether a is less than or equal to b
C: <= ( a b -- f )
    f = (a <= b) ? TRUE : FALSE;
;C

\ Test whether a and b are equal
C: = ( a b -- f )
    f = (a == b) ? TRUE : FALSE;
;C

\ Test whether a and b are unequal
C: <> ( a b -- f )
    f = (a == b) ? FALSE : TRUE;
;C
	
\ Test whether a is zero
C: 0= ( a -- f )
    f = (a == 0) ? TRUE : FALSE;
;C

\ Test whether a is non-zero
C: 0<> ( a -- f )
    f = (a != 0) ? TRUE : FALSE;
;C

\ Test whether a is less than zero
C: 0< ( a -- f )
    f = (a < 0) ? TRUE : FALSE;
;C

\ Test whether a is less than or equal to zero
C: 0<= ( a -- f )
    f = (a <= 0) ? TRUE : FALSE;
;C

\ Test whether a is greater than zero
C: 0> ( a -- f )
    f = (a > 0) ? TRUE : FALSE;
;C

\ Test whether a is greater than or equal to zero
C: 0>= ( a -- f )
    f = (a >= 0) ? TRUE : FALSE;
;C


\ ---------- Bit-level operators ----------

\ Bitwise AND
C: AND ( a b -- c )
    c = a & b;
;C

\ Bitwise OR
C: OR ( a b -- c )
    c = a | b;
;C

\ Bitwise XOR
C: XOR ( a b -- c )
    c = a ^ b;
;C

\ Bitwise inversion
C: INVERT ( a -- b )
    b  = ~a;
;C

\ Logical negation -- kind of unnecessary, but makes code easier to read
C: NOT ( f -- nf )
    nf = f ? FALSE : TRUE;
;C
    
\ Shift a left n bits, replacing the rightmost n bits with zeros
C: LSHIFT ( a n -- b )
    b = (CELL) (((UCELL) a) << n);
;C

\ Shift a right n bits, replacing the leftmost n bits with zeros
C: RSHIFT ( a n -- b )
    b = (CELL) (((UCELL) a) >> n);
;C


\ ---------- Common operations ----------
\ sd: For these to be worthwhile defining as primitives they need to be more optimised

\ Increment the top stack item    
C: 1+ ( n1 -- n2 )
    n2 = n1 + 1;
;C

\ Decrement the top stack item    
C: 1- ( n1 -- n2 )
    n2 = n1 - 1;
;C

\ Double the top stack item    
C: 2* ( n1 -- n2 )
    n2 = n1 << 1;
;C

\ Halve the top stack item    
C: 2/ ( n1 -- n2 )
    n2 = n1 >> 1;
;C


\ ---------- Memory operations ----------

\ Store a value in a cell
C: ! prim_store_cell ( v addr -- )
    (*((CELLPTR) addr)) = (CELL) v;
;C

\ Retrieve the cell at the given address
C: @ prim_fetch_cell ( addr -- v )
    v = (*((CELLPTR) addr));
;C
	
\ Increment the value at the given address
C: +! ( n addr -- )
    (*((CELLPTR) addr)) += n;
;C
    
\ Store a character at an address
C: C! ( c addr -- )
    (*((CHARACTERPTR) addr)) = (CHARACTER) c;
;C

\ Store a double cell value
C: 2! ( a b addr -- )
    CELLPTR ptr;

    ptr = (CELLPTR) addr;
    *ptr++ = b;
    *ptr = a;
;C

\ Retrieve a double cell from the given address
C: 2@ ( addr -- a b )
    CELLPTR ptr;

    ptr = (CELLPTR) addr;
    b = *ptr++;
    a = *ptr;
;C

\ Retrieve the character at the given address
C: C@ ( addr -- c )
    c = (*((CHARACTERPTR) addr));
;C

\ sd: These next two words are deliberately coded without using memcpy()
\ or bcopy() to minimise library dependencies    
    
\ Move a block of memory from addr1 to addr2, starting from the lower address
C: CMOVE ( addr1 addr2 n -- )
    int i;
    BYTEPTR ptr1, ptr2;

    ptr1 = (BYTEPTR) addr1;    ptr2 = (BYTEPTR) addr2;
    for(i = 0; i < n ; i++) {
        *ptr2++ = *ptr1++;
    }
;C

\ Move a block of memory from addr1 to addr2, starting from the higher address
C: CMOVE> ( addr1 addr2 n -- )
    int i;
    BYTEPTR ptr1, ptr2;

    ptr1 = (BYTEPTR) ((BYTEPTR) addr1 + n - 1);    ptr2 = (BYTEPTR) ((BYTEPTR) addr2 + n - 1);
    for(i = 0; i < n ; i++) {
        *ptr2-- = *ptr1--;
    }
;C

