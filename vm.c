// $Id: vm.c,v 1.6 2007/06/13 15:57:39 sd Exp $

// This file is part of Attila, a multi-targeted threaded interpreter
// Copyright (c) 2007, UCD Dublin. All rights reserved.
//
// Attila is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// Attila is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

// Virtual machine primitives


#include "attila.h"

// ---------- Abstract machine "registers" ----------

CELLPTR ip;
XT current_xt;

static char start_buffer[255];
char *start = &start_buffer;


// ---------- Inner interpreter ----------

// Placeholder for a return from a word
// PRIMITIVE NEXT ( -- )
void
next() {
}


// Execute the word whose xt is on the data stack
// PRIMITIVE EXECUTE ( xt -- )
void
execute() {
  PRIMITIVE code;

  // get the next xt
  POP_CELL(current_xt);
  // printf("%x\n", current_xt);

  // grab the code field and code of the word
  code = xt_to_codeheader(current_xt)->cfa;

  // if code field is NEXT, pop the return address and continue
  if(code == &next) {
    POP_RETURN(ip);
    return;
  }

  // if the code is a primitive, execute it, otherwise make a jump
  // to the code
  if(code != &inner_interpreter) {
    (*code)();        // call the primitive
  } else {
    PUSH_RETURN(ip);
    ip = xt_to_code(current_xt);
  }
}


// The inner interpreter, as a single non-terminating loop. Each iteration
// works by grabbing an XT from the current address, pushing it onto the
// data stack, and then executing it using the same code as the EXECUTE
// word uses at user level -- although the primitive is used directly. This
// is a little inefficient as it involves going through the data stack
// on every virtual machine cycle, so it might be better to optimise later
// PRIMITIVE (DOCOLON) ( -- )
void
inner_interpreter() {
  XT xt;

  do {
    // grab the execution token
    xt = *((XT *) ip);
    
    // point the instruction pointer at the start of
    // the code portion of the word
    // ip = xt_to_code(xt);
    ++ip;

    // push the xt into the data stack and execute it
    PUSH_CELL(xt);
    execute();
  } while(1);
}


// A no-op
// PRIMITIVE NOOP ( -- )
void
noop() {
}


// Re-start the virtual machine
// PRIMITIVE RESTART ( -- )
void
restart() {
  longjmp(env, 0);
}


// Re-set the system by emptying the return stack, resetting the input
// source and system state, and calling the executive
// PRIMITIVE WARM ( -- )
void
warm() {
  CELLPTR exec;
  FILE *of;
  char **tib;
  int *offset;

  // reset the stacks
  stack_reset(data_stack, &data_stack_top);
  stack_reset(return_stack, &return_stack_top);

  // close any open input file
  of = (FILE *) (*(user_variable(USER_INPUTSOURCE)));
  if(of != stdin)
    fclose(of);

  // reset the input source and user variables
  *(user_variable(USER_BASE)) = 10;                    // base 10 numbers
  *(user_variable(USER_STATE)) = STATE_INTERPRETING;   // interpreter state
  *(user_variable(USER_INPUTSOURCE)) = stdin;          // terminal input

  // if we're cold-starting, run the start-up code
  offset = (int *) user_variable(USER_OFFSET);
  if(start[0] != '\0') {
    tib = (char **) user_variable(USER_TIB);   
    strcpy(*tib, start);   *offset = 0;
    start[0] = '\0';
  } else {
    *offset = -1;
  }

  // jump to the executive
  exec = user_variable(USER_EXECUTIVE);
  ip = exec;
}


// Exit the interpreter
// PRIMITIVE BYE ( -- )
void
bye() {
  exit(0);
}


// ---------- Machine constants and data type sizes ----------

// The size of a cell
// PRIMITIVE CELL ( -- size )
void
cell() {
  CELL size = CELL_SIZE;
  PUSH_CELL(size);
}


// ---------- User variables ----------

// Return the address of a user variable
// PRIMITIVE USERVAR ( n -- addr )
void
uservar() {
  CELL n;
  CELLPTR ptr;

  POP_CELL(n);
  ptr = user_variable(n);
  PUSH_CELL(ptr);
}


// ---------- Data stack ----------

// Duplicate the top element of the stack
// PRIMITIVE DUP ( n -- n n )
void
one_dup() {
  CELL n;
  PEEK_CELL(n);
  PUSH_CELL(n);
}


// Duplicate the top two elements of the stack
// PRIMITIVE 2DUP ( n m -- n m n m )
void
two_dup() {
  CELL n, m;
  PICK_CELL(1, n);
  PEEK_CELL(m);
  PUSH_CELL(n);
  PUSH_CELL(m);
}


// Duplicate the top element of the stack if it's non-zero
// PRIMITIVE ?DUP ( n | 0 -- n n | 0 )
void
query_dup() {
  CELL a;
  PEEK_CELL(a);
  if(a != 0)
    PUSH_CELL(a);
}


// Drop the top item
// PRIMITIVE DROP ( n -- )
void
drop() {
  CELL n;
  POP_CELL(n);
}


// Drop the top two items
// PRIMITIVE 2DROP ( n m -- )
void
two_drop() {
  CELL n;
  POP_CELL(n);
  POP_CELL(n);
}


// Swap the top two items
// PRIMITIVE SWAP ( n m -- m n)
void
swap() {
  stack_roll(data_stack, &data_stack_top, 1, CELL_SIZE);
}


// Copy the second item over the first
// PRIMITIVE OVER ( n m -- n m n )
void
over() {
  CELL n;
  PICK_CELL(1, n);
  PUSH_CELL(n);
}


// Rotate the stack clockwise, bringing the second item to the top
// PRIMITIVE ROT ( a b c -- c a b )
void
rot() {
  stack_roll(data_stack, &data_stack_top, 2, CELL_SIZE);
}


// Rotate the stack anti-clockwise, bringing the second item to the top
// PRIMITIVE -ROT ( a b c -- b c a )
void
minus_rot() {
  stack_backroll(data_stack, &data_stack_top, 2, CELL_SIZE);
}


// Roll the stack clockwise by i elements, not counting the count, starting from 0
// PRIMITIVE ROLL ( ni+1 ... n0 i -- n0 ni-1 ... n1 )
void
roll() {
  CELL i;
  POP_CELL(i);
  stack_roll(data_stack, &data_stack_top, i, CELL_SIZE);
}


// Roll the stack anti-clockwise by i elements, not counting the count,
// starting from 0
// PRIMITIVE -ROLL ( ni+1 ... n0 i -- ni ... n1 n0 ni-1 )
void
backroll() {
  CELL i;
  POP_CELL(i);
  stack_backroll(data_stack, &data_stack_top, i, CELL_SIZE);
}


// Pick the n'th item down, numbered from before the top item (not including
// the count tiem itself), starting at 0 -- so 0 PICK = DUP, 1 PICK == OVER
// PRIMITIVE PICK ( ... n2 n1 n0 i -- ... n2 n1 n0 ni )
void
pick() {
  CELL i, n;
  POP_CELL(i);
  PICK_CELL(i, n);
  PUSH_CELL(n);
}


// Remove the second stack element
// PRIMITIVE NIP ( a b -- b )
void
nip() {
  CELL a, b;
  POP_CELL(b);   POP_CELL(a);
  PUSH_CELL(b);
}


// Tuck the top value under the second
// PRIMITIVE TUCK ( a b -- b a b )
void
tuck() {
  CELL a, b;
  POP_CELL(b);   POP_CELL(a);
  PUSH_CELL(b);   PUSH_CELL(a);   PUSH_CELL(b);
}


// Push the depth of the stack, not including the depth itself
// PRIMITIVE DEPTH ( -- d )
void
hash_stack() {
  CELL d = DEPTH_CELL();
  PUSH_CELL(d);
}


// ---------- Return stack ----------

// Push a cell onto the return stack
// PRIMITIVE >R ( addr -- )
void
to_r() {
  CELL c;
  POP_CELL(c);
  PUSH_RETURN(c);
}


// Pop a cell off the return stack
// PRIMITIVE R> ( -- addr )
void
from_r() {
  CELL c;
  POP_RETURN(c);
  PUSH_CELL(c);
}


// Peek at the top of the return stack
// PRIMITIVE R@ ( -- addr )
void
peek_r() {
  CELL c;
  PEEK_RETURN(c);
  PUSH_CELL(c);
}


// Pop the i'th address of the return stack, starting at 0
// PRIMITIVE RPICK ( i -- n )
void
rpick() {
  CELL i;
  CELL n;
  POP_CELL(i);
  PICK_RETURN(i, n);
  PUSH_CELL(n);
}


// Drop the top item on the return stack
// PRIMITIVE RDROP ( -- )
void
rdrop() {
  CELL addr;
  POP_RETURN(addr);
}


// ---------- Logic and tests ----------

// Logical not
// PRIMITIVE NOT ( a -- ~a )
void
logical_not() {
  CELL a, b;

  POP_CELL(a);
  b = a ? 0 : 1;
  PUSH_CELL(b);
}


// ---------- Control structure builders ----------

// Place the current data address onto the top of the
// data stack
// PRIMITIVE HERE ( -- addr )
void
here() {
  byte *here = allocate_word_data_memory(0);
  PUSH_CELL(here);
}


// Place the current code-space pointer onto the top
// of the stack
// PRIMITIVE TOP ( -- addr )
void
top() {
  byte *top = allocate_word_code_memory(0);
  PUSH_CELL(top);
}


// Jump unconditionally to the address compiled in the next cell
// PRIMITIVE (BRANCH) ( -- )
void
prim_branch() {
  CELL offset = *((CELLPTR *) ip);         // jump to the embedded offset
  ip = (CELLPTR) (((byte *) ip) + offset);
}


// Test the top of the stack and either continue (if true) or
// jump to the address compiled in the next cell (if false)
// PRIMITIVE (?BRANCH) ( f -- )
void
prim_branch_or_continue() {
  CELL f;
  POP_CELL(f);
  if(f)
    ip++;                                    // jump the embedded offset to continue
  else {
    CELL offset = *((CELLPTR *) ip);         // jump to the embedded offset
    ip = (CELLPTR) (((byte *) ip) + offset);
  }
}


// ---------- Memory access ----------

// Grab a cell from the address
// PRIMITIVE @ ( addr -- v )
void
fetch() {
  CELLPTR addr;
  CELL v;

  POP_CELL(addr);
  v = *addr;
  PUSH_CELL(v);
}


// Store the value in the address
// PRIMITIVE ! ( v addr -- )
void
store() {
  CELLPTR addr;
  CELL v;

  POP_CELL(addr);
  POP_CELL(v);
  *addr = v;
}


// Grab a character from the address
// PRIMITIVE C@ ( addr -- c )
void
c_fetch() {
  CELLPTR addr;
  CELL v;

  POP_CELL(addr);
  v = (CELL) (*((char *) addr));
  PUSH_CELL(v);
}


// Store the character  in the address
// PRIMITIVE C! ( c addr -- )
void
c_store() {
  CELLPTR addr;
  CELL c;

  POP_CELL(addr);
  POP_CELL(c);
  *((char *) addr) = (char) c;
}


// Fast increment of the cell at the given address
// PRIMITIVE +! ( n addr -- )
void
plus_store() {
  CELLPTR addr;
  CELL n;

  POP_CELL(addr);   POP_CELL(n);
  *addr += n;
}


// ---------- Field access ----------

// Convert the xt of a word to its body address
// PRIMITIVE >BODY ( xt -- addr )
void
tto_body() {
  XT xt;
  POP_CELL(xt);
  CELLPTR body = xt_to_body(xt);
  PUSH_CELL(body);
}


// Convert an xt to a cfa
// PRIMITIVE >CFA ( xt -- cfa )
void
tto_cfa() {
  CELL xt;
  CELLPTR cfa;

  POP_CELL(xt);
  cfa = &xt_to_codeheader(xt)->cfa;
  PUSH_CELL(cfa);
}


// Convert an xt to a status address
// PRIMITIVE >STATUS ( xt -- statusaddr )
void
tto_status() {
  CELL xt;
  byte *status;

  POP_CELL(xt);
  status = &xt_to_codeheader(xt)->status;
  PUSH_CELL(status);
}


// Convert an xt to the address of its code block. This does not
// guarantee there is actually code there, and anyway we shouldn't
// encourage access to the compiled code...
// PRIMITIVE >CODE ( xt -- code )
void
to_code() {
  CELL xt;
  CELLPTR code;

  POP_CELL(xt);
  code = xt_to_code(xt);
  PUSH_CELL(code);
}


// ---------- Compilation ----------

// Return the address of the word's body, taking account of redirectability
// PRIMITIVE (DOVAR) ( -- addr )
void
do_var() {
  CELLPTR body = xt_to_body(current_xt);
  PUSH_CELL(body);
}


// For a re-directable word, grab the indirect body address and jump to it,
// pushing the real body address onto the stack first
// sd: should we combine this with (DOVAR) and switch on redirectability?
// PRIMITIVE (DOES) ( -- body )
void
do_does() {
  CELLPTR body = xt_to_body(current_xt);
  CELLPTR iba = body - 1;
  PUSH_CELL(body);
  PUSH_RETURN(ip);
  ip = *((CELLPTR *) iba);
}


// Push a literal onto the stack, stored in the next cell
// PRIMITIVE (LITERAL) ( -- n )
void
do_literal() {
  CELL n = *ip;
  PUSH_CELL(n);
  ip++;          // jump the embedded literal
}


// Compile an xt into the code area
// PRIMITIVE COMPILE, ( xt -- )
void
compile_comma() {
  CELL n;
  POP_CELL(n);
  compile_xt(n);
}


// Compile a string into the code area, used for literal strings
// PRIMITIVE SCOMPILE, ( addr len -- )
void
scompile_comma() {
  CELL addr;
  CELL len;
  byte l;

  POP_CELL(len);
  POP_CELL(addr);
  l = (byte) len;   compile_code(&l, sizeof(byte));
  compile_code(addr, len);
}


// Push a literal string's address and length onto the stack,
// store in the next bytes. Strings can be at most 256 characters long
// PRIMITIVE (SLITERAL) ( -- addr len )
void
do_string_literal() {
  char *ptr = (char *) ip;
  CELL len = *((byte *) ptr);
  CELL addr = (CELL) (++ptr);
  PUSH_CELL(addr);
  PUSH_CELL(len);
  ip = (CELLPTR) (ptr + len);   // jump past the string
}


// Compile the top stack item at the top of the data area
// PRIMITIVE , ( n -- )
void
comma() {
  CELL n;
  POP_CELL(n);
  compile_data(&n, sizeof(CELL));
}


// Compile the top stack item as a character into the data area
// PRIMITIVE C, ( n -- )
void
char_comma() {
  CELL n;
  byte b;
  POP_CELL(n);
  b = (byte) n;   compile_data(&n, sizeof(byte));
}


// Compile a string into the data area
// PRIMITIVE S, ( addr len -- )
void
string_comma() {
  CELL len;
  byte *addr;

  POP_CELL(len);
  POP_CELL(addr);
  compile_data(&len, sizeof(byte));
  compile_data(addr, len);
}


// Allot n bytes in the data area, without initialising them
// PRIMITIVE ALLOT ( n -- )
void
allot() {
  CELL n;

  POP_CELL(n);
  allocate_word_data_memory(n);
}


// ---------- Debugging support only ----------

// Print the top number on the stack
// PRIMITIVE DUMP. ( n -- )
void
debug_dump_dot() {
  CELL n;
  POP_CELL(n);
  printf("%d ", n);
}


// Dump the stack
// PRIMITIVE DUMPS. ( -- )
void
dump_stack() {
  int n = DEPTH_CELL();
  int i;
  CELL c;

  for(i = n - 1; i > 0; i--) {
    PICK_CELL(i, c);
    printf("%d ", c);
  }
}


// Display the memory beginning at addr for n cells
// PRIMITIVE DUMP ( addr n -- )
void
debug_dump() {
  CELLPTR addr;
  CELL n;
  int i;
  CELL v;

  POP_CELL(n);   POP_CELL(addr);
  for(i = 0; i < n ; i++) {
    v = *addr;
    printf("%x: %d\n", addr++, v);
  }
}

