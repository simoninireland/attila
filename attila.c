// $Id: attila.c,v 1.7 2007/06/13 15:57:39 sd Exp $

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

// Attila main setup and interactive routine

#include "attila.h"


// ---------- Globals ----------

byte *data_stack, *data_stack_top;
byte *return_stack, *return_stack_top;
jmp_buf env;

char *prelude = ATTILA_STANDARD_PRELUDE;

// ---------- Main routine ----------

int
main( int argc, char **argv ) {
  // announce ourselves
  printf("%s\n", ATTILA_ANNOUNCE);
  printf("%s\n", ATTILA_COPYRIGHT);

  // add the standard include directory
  include_path = (char *) calloc(sizeof(char *), MAX_INCLUDE_PATH);
  include_path[includes++] = ATTILA_STANDARD_INCLUDE_DIR;

  // process options
  char c;
  while((c = getopt(argc, argv, "I:np:h")) != -1) {
    switch(c) {
    case 'I': {
      // add a directory to the include path
      char *d = strdup(optarg);
      if(includes >= MAX_INCLUDE_PATH) {
	fprintf(stderr, "Too many included directories: %s ignored\n", d);
	free(d);
      } else {
	include_path[includes++] = d;
      }
      break;
    }

    case 'n':
      // do not load the standard prelude
      prelude = NULL;
      break;

    case 'p':
      // use the given file as the standard prelude instead of prelude.fs
      prelude = optarg;
      break;

    case '?':
    case 'h':
      // display help and exit
      printf("attila: an abstract multi-targeted threaded interpreter\n");
      printf("usage: attila [options] [source ...]\n");
      printf("options:\n");
      printf("   -I <dir>      add <dir> to the include path\n");
      printf("   -n            do not load standard prelude\n");
      printf("   -p <prelude>  use <prelude> as the prelude instead\n");
      printf("                 of prelude.fs\n");
      printf("   -h            display usage\n");
      exit(0);
    }
  }
  argc -= optind;
  argv += optind;

  // set up the starting command, #include'ing the prelude (if required) followed
  // by all the file mentioned on the command line
  if(prelude != NULL) {
    sprintf(start, "#include %s", prelude);
  } else {
    start[0] = '\0';
  }
  int i;
  for(i = 0; i < argc; i++) {
    strcat(start, " #include ");
    strcat(start, argv[i]);
    //    strcat(start, " ");
  }
  printf("%s\n", start);

  // set up inital (root) vocabulary, dictionary, data and return stacks
  rootvocabulary = new_vocabulary(NAMESPACE_MEMORY_SIZE, CODESPACE_MEMORY_SIZE, DATASPACE_MEMORY_SIZE, NULL);
  *(user_variable(USER_CURRENT)) = rootvocabulary;
  stack_init(&return_stack, &return_stack_top, RETURN_STACK_SIZE);
  stack_init(&data_stack, &data_stack_top, DATA_STACK_SIZE);
  *(user_variable(USER_TIB)) = (char *) allocate_word_data_memory(TIB_SIZE);
  *(user_variable(USER_OFFSET)) = -1;

  // create the dictionary and initialise the user variables
  *(user_variable(USER_INPUTSOURCE)) = stdin;
  build_bootstrap_dictionary();
  build_initial_dictionary();

  // set the executive and re-start point
  *(user_variable(USER_EXECUTIVE)) = xt_of_word("OUTER");
  XT warm = xt_of_word("WARM");

  // we return here in case of catastrophic errors
  setjmp(env);
  stack_reset(data_stack, &data_stack_top);
  stack_reset(return_stack, &return_stack_top);

  // boot
  ip = &warm;
  inner_interpreter();

  exit(0);
}
