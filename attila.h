// $Id$

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

// Basic header definitions

#ifndef ATTILA_H
#define ATTILA_H

#include <config.h>
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <strings.h>
#include <setjmp.h>
#include <limits.h>


// ---------- Sizes etc ----------

#define CELL_SIZE ((size_t) SIZEOF_INT)
#define STATUS_SIZE ((size_t) SIZEOF_UNSIGNED_CHAR)

#define MAX_INCLUDE_PATH 20
#define NAMESPACE_MEMORY_SIZE (64 * 1024)
#define CODESPACE_MEMORY_SIZE (64 * 1024)
#define DATASPACE_MEMORY_SIZE (64 * 1024)
#define DATA_STACK_SIZE (5 * 1024)
#define RETURN_STACK_SIZE (2 * 1024)
#define TIB_SIZE 1024
#define SCRATCH_SIZE 100
#define USER_SIZE (8 * CELL_SIZE)


// ---------- Types ----------

typedef unsigned char boolean;
typedef unsigned char byte;

// cells
typedef int CELL;
typedef CELL *CELLPTR;

// word layout
typedef void (*PRIMITIVE)();
typedef struct code_struct {
  PRIMITIVE cfa;
  byte status;
  CELLPTR body;
  /* CELL code[]; */
} CODE;
typedef CODE *CODEPTR;
typedef CELLPTR XT;
typedef struct header_struct {
  struct header_struct *lfa;
  CODEPTR codeheader;
  byte namelen;
  /* byte name[]; */
} HEADER;
typedef HEADER *HA;

// vocabularies
typedef struct vocabulary_struct {
  size_t namespace_size;
  byte *namespace;
  byte *head;
  size_t codespace_size;
  byte *codespace;
  byte *top;
  size_t dataspace_size;
  byte *dataspace;
  byte *here;
  char *ourname;
  struct vocabulary_struct *ourvocabulary;
  HA lastha;
  XT lastxt;
} VOCABULARY;
typedef VOCABULARY *VA;


// ---------- User variable offsets ----------

#define USER_EXECUTIVE   0  // the xt of the outer executive (OUTER by default)
#define USER_STATE       1  // interpreting or compiling
#define USER_BASE        2  // the number base
#define USER_INPUTSOURCE 3  // file pointer to the input source
#define USER_CURRENT     4  // the current vocabulary receiving definitions
#define USER_TIB         5  // the input buffer
#define USER_OFFSET      6  // the current character offset within the buffer

// ---------- States ----------

#define STATE_INTERPRETING 0  // system is interpreting
#define STATE_COMPILING    1  // system is compiling


// ---------- Status flags ----------

#define STATUS_IMMEDIATE      1  // IMMEDIATE (execute when compiling)
#define STATUS_REDIRECTABLE   2  // can be re-directed
#define STATUS_COMPILABLE     4  // can be compiled (net yet used)
#define STATUS_INTERPRETABLE  8  // can be interpreted (not yet used)

#define STATUS_DEFAULT        (STATUS_COMPILABLE | STATUS_INTERPRETABLE)


// ---------- Prototypes ----------

// Virtual machine
extern CELLPTR ip;                              // instruction pointer
extern byte *data_stack, *data_stack_top;       // data stack
extern byte *return_stack, *return_stack_top;   // return stack
extern jmp_buf env;                             // restart point
extern VA rootvocabulary;                       // root vocabulary

// Start-up code
extern char *start;

// Source inclusions
extern char **include_path;
extern int includes;

// Memory and dictionary management
extern VA new_vocabulary( size_t ns, size_t cs, size_t ds, VA parent );
extern HA allocate_word_header_memory( size_t n );
extern byte *allocate_word_code_memory( size_t n );
extern byte *allocate_word_data_memory( size_t n );
extern HA begin_word( byte *name, int namelen );
extern byte *compile_code( byte *addr, int len );
extern XT *compile_xt( XT xt );
extern byte *compile_data( byte *addr, int len );
extern CELLPTR compile_data_cell( CELL v );
extern byte *allot_data( int len );
extern byte *ha_to_name( HA ha );
extern XT ha_to_xt( HA ha );
extern CODEPTR xt_to_codeheader( XT xt );
extern XT *xt_to_code( XT xt );
extern CELLPTR xt_to_body( XT );
extern CELLPTR user_variable( int i );

// Bootstrapping
extern XT compile_primitive( char *name, PRIMITIVE prim, boolean immediate );
extern XT begin_colon_definition( char *name, boolean immediate );
extern XT begin_create( char *name );
extern XT xt_of_word( char *name );
extern XT compile_xt_of( char *name );
extern void end_colon_definition();
extern byte *compile_string( char *str );
extern void build_bootstrap_dictionary();

// Utility
extern char *til_string_to_c_string( char *str, int n );

// Stack
extern void stack_init( byte **st, byte **top, size_t len );
extern void stack_push( byte *st, byte **top, void *elem, size_t n );
extern void stack_pop( byte *st, byte **top, void *elem, size_t n );
extern void stack_peek( byte *st, byte **top, void *elem, size_t n );
extern void stack_topptr( byte *st, byte **top, void **elem, size_t n );
extern void stack_pick( byte *st, byte **top, int i, void *elem, size_t n );
extern void stack_roll( byte *st, byte **top, int i, size_t n );
extern void stack_backroll( byte *st, byte **top, int i, size_t n );
extern int stack_depth( byte *st, byte **top, size_t n );
extern void stack_reset( byte *st, byte **top );

// Important primitives
extern void inner_interpreter();
extern void bye();
extern void restart();
extern void do_var();
extern void noop();


// ---------- Helper macros ----------

#define CRASH(msg) printf(msg); restart()

#define PUSH_CELL(var) stack_push(data_stack, &data_stack_top, (void *) &var, CELL_SIZE)
#define POP_CELL(var) stack_pop(data_stack, &data_stack_top, (void *) &var, CELL_SIZE)
#define PEEK_CELL(var) stack_peek(data_stack, &data_stack_top, (void *) &var, CELL_SIZE)
#define TOPPTR_CELL(var) stack_topptr(data_stack, &data_stack_top, (void **) &var, CELL_SIZE)
#define PICK_CELL(i, var) stack_pick(data_stack, &data_stack_top, i, (void *) &var, CELL_SIZE)
#define DEPTH_CELL() stack_depth(data_stack, &data_stack_top, CELL_SIZE)

#define PUSH_RETURN(var) stack_push(return_stack, &return_stack_top, (void *) &var, CELL_SIZE)
#define POP_RETURN(var) stack_pop(return_stack, &return_stack_top, (void *) &var, CELL_SIZE)
#define PEEK_RETURN(var) stack_peek(return_stack, &return_stack_top, (void *) &var, CELL_SIZE)
#define PICK_RETURN(i, var) stack_pick(return_stack, &return_stack_top, i, (void *) &var, CELL_SIZE)

#endif
