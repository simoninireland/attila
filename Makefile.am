# $Id$

# This file is part of Attila, a retargetable threaded interpreter
# Copyright (c) 2007--2010, Simon Dobson <simon.dobson@computer.org>.
# All rights reserved.
#
# Attila is free software; you can redistribute it and/or
# modify it under the terms of the GNU General Public License
# as published by the Free Software Foundation; either version 2
# of the License, or (at your option) any later version.
#
# Attila is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

# ----- Main programs -----

bin_PROGRAMS = bootattila attila


# ----- Supporting programs and scripts -----

EXTRA_DIST = tools/primgen \
	tools/crosscompile.fs \
	tools/test.fs


# ----- Sources -----

# The bootstrapping sources
bootattila_SOURCES = vm.h \
	primgen.h \
	bootstrap.fs \
	bootstrap.h \
	bootstrap.c \
	bootattila.c \
	bootstrap-prelude.fs \
	bootstrap-vm.fs

# The generated primitives file
nodist_bootattila_SOURCES = primitives.c

# The cross-compiler sources and generated VM
attila_SOURCES = x-host.h
nodist_attila_SOURCES = attila.c


# ----- The bootstrapped interpreter primitives file -----

primitives.c: bootstrap.fs c-core.fs c-fileio.fs c-io.fs
	$(PERL) $(srcdir)/tools/primgen -o primitives.c $(srcdir)/bootstrap.fs $(srcdir)/c-core.fs $(srcdir)/c-fileio.fs $(srcdir)/c-io.fs


# ----- The cross-compiled interpreter VM -----

attila.c: bootattila
	cd $(srcdir) && $(abs_builddir)/bootattila <$(abs_builddir)/tools/crosscompile.fs


# ----- Self-testing -----

check: test

test: attila
	cd $(srcdir) && $(abs_builddir)/attila <$(srcdir)/tools/test.fs

.PHONY: test


# ----- The system sources -----

# All the Forth code intended for normal use
forth = prelude.fs \
	comments.fs \
	conditionals.fs \
	base.fs \
	createdoes.fs \
	interpret-compile.fs \
	variables.fs \
	uservariables.fs \
	constants.fs \
	stringconstants.fs \
	values.fs \
	defer.fs \
	vm.fs \
	ascii.fs \
	bootstrap-loops.fs \
	stacks.fs \
	cs-stack.fs \
	loop-support.fs \
	loops.fs \
	counted-loops-runtime.fs \
	counted-loops.fs \
	case.fs \
	hooks-runtime.fs \
	hooks.fs \
	conditional-compilation.fs \
	chars.fs \
	strings.fs \
	zstrings.fs \
	type.fs \
	parser.fs \
	scratch.fs \
	formatting.fs \
	lists.fs \
	records.fs \
	file.fs \
	evaluate.fs \
	c.fs \
	wordlists.fs \
	dynamic-memory.fs \
	decompiler.fs \
	funit.fs

# The cross-compiler
crosscompiler = x-host.h \
	cross-compiler/cross-compiler.fs \
	cross-compiler/cross-locator.fs \
	cross-compiler/cross-flat-memory-model.fs \
	cross-compiler/cross-image-fixedsize.fs \
	cross-compiler/cross-c.fs \
	cross-compiler/cross-colon.fs \
	cross-compiler/cross-c-vm.fs \
	cross-compiler/cross-c-generate.fs \
	cross-compiler/cross-environments.fs

# The system sources, used to cross-compile a basic virtual machine
regular = c-core.fs \
	c-itil.fs \
	itil.fs \
	c-fileio.fs \
	c-io.fs \
	system-hooks.fs \
	flat-memory-model.fs \
	itil-compilation.fs \
	compilation.fs \
	colon.fs \
	load.fs \
	include.fs \
	executive.fs \
	startup.fs

# Tests
test = test/core-system-tests.fs \
	test/stack.fs \
	test/arithmetic.fs \
	test/words.fs \
	test/allot.fs \
	test/ascii.fs \
	test/strings.fs \
	test/formatting.fs \
	test/loops.fs \
	test/conditional-compilation.fs \
	test/hooks.fs \
	test/parser.fs

# ----- Distribution -----

nobase_dist_pkgdata_DATA = $(forth) $(crosscompiler) $(regular) $(test)
DISTCLEANFILES = primitives.c attila.c

