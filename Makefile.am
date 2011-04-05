# $Id$

# This file is part of Attila, a retargetable threaded interpreter
# Copyright (c) 2007--2011, Simon Dobson <simon.dobson@computer.org>.
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

# toplevel programs
bin_PROGRAMS = bootattila attila


# ----- Supporting programs and scripts -----

# tools used to bootstrap system
EXTRA_DIST = \
	target/host/x-host-initial.fs.in\
	target/host/x-host.fs.in\
	tools/primgen \
	tools/crosscompile.fs \
	tools/test.fs


# ----- Sources -----

# bootstrapping sources
bootattila_SOURCES = vm.h \
	primgen.h \
	bootstrap.fs \
	bootstrap.h \
	bootstrap.c \
	bootattila.c \
	bootstrap-prelude.fs \
	bootstrap-vm.fs

# generated primitives file
nodist_bootattila_SOURCES = primitives.c

# cross-compiler sources and generated VM
attila_SOURCES = 
nodist_attila_SOURCES = attila.c


# ----- The bootstrapped interpreter primitives file -----

# primitives generated from C/Forth code
primitives.c: bootstrap.fs stdc/stdc-core.fs stdc/stdc-fileio.fs
	$(PERL) $(srcdir)/tools/primgen -o primitives.c $(srcdir)/bootstrap.fs $(srcdir)/stdc/stdc-core.fs $(srcdir)/stdc/stdc-fileio.fs


# ----- The cross-compiled interpreter VM -----

# substitutions for directories in host cross-compiler driver
dirs = sed \
	-e 's,@datadir\@,$(pkgdatadir),g' \
	-e 's,@abs_srcdir\@,$(abs_srcdir),g' \
	-e 's,@abs_builddir\@,$(abs_builddir),g'

# host development system initial cross-compiler driver
target/host/x-host-initial.fs: target/host/x-host-initial.fs.in
	$(dirs) $(srcdir)/target/host/x-host-initial.fs.in >$(abs_builddir)/target/host/x-host-initial.fs

# host development system general cross-compiler driver
target/host/x-host.fs: target/host/x-host.fs.in
	$(dirs) $(srcdir)/target/host/x-host.fs.in >$(abs_builddir)/target/host/x-host.fs

# cross-compiled system image
attila.c: bootattila $(regular) target/host/x-host-initial.fs
	cd $(srcdir) && $(abs_builddir)/bootattila <$(abs_builddir)/tools/crosscompile.fs


# ----- Self-testing -----

# self-tests run on installation
check: test

# test suite
test: attila
	cd $(srcdir) && $(abs_builddir)/attila <$(srcdir)/tools/test.fs

.PHONY: test


# ----- The system sources -----

# all the Forth code intended for normal use
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
	bootstrap-loops.fs \
	stacks.fs \
	cs-stack.fs \
	loop-support.fs \
	loops.fs \
	counted-loops-runtime.fs \
	counted-loops.fs \
	continuations.fs \
	case.fs \
	hooks.fs \
	conditional-compilation.fs \
	chars.fs \
	strings.fs \
	zstrings.fs \
	scratch.fs \
	formatting.fs \
	lists.fs \
	records.fs \
	evaluate.fs \
	c.fs \
	wordlists.fs \
	dynamic-memory.fs \
	decompiler.fs \
	funit.fs

# cross-compiler
crosscompiler = \
	cross-compiler/cross-target.fs \
	cross-compiler/cross-compiler.fs \
	cross-compiler/cross-locator.fs \
	cross-compiler/cross-flat-memory-model.fs \
	cross-compiler/cross-image-fixedsize.fs \
	cross-compiler/cross-c.fs \
	cross-compiler/cross-colon.fs \
	cross-compiler/cross-c-vm.fs \
	cross-compiler/cross-c-generate.fs \
	cross-compiler/cross-environments.fs \
	cross-compiler/x-standard.fs

# host development system additional sources
host = \
	target/host/x-host.fs \
	target/host/vm.fs

# standard runtime
runtime = \
	stdc/stdc-types-standard.h \
	stdc/stdc-stacks-bare.h \
	stdc/stdc-main.h \
	stdc/stdc-runtime-standard.h
 
# system sources, used to cross-compile a basic virtual machine
regular = \
	stdc/stdc-core.fs \
	stdc/stdc-segment.fs \
	stdc/stdc-flat-memory-model-runtime.fs \
	stdc/stdc-itil.fs \
	bootstrap-prelude.fs \
	itil.fs \
	stdc/stdc-continuations-runtime.fs \
	stdc/stdc-fileio.fs \
	ascii.fs \
	chain.fs \
	hooks-runtime.fs \
	system-hooks.fs \
	flat-memory-model.fs \
	itil-compilation.fs \
	words-itil-common.fs \
	words-itil-named.fs \
	words-itil-unnamed.fs \
	warm.fs \
	abort.fs \
	io.fs \
	type.fs \
	parser.fs \
	colon.fs \
	compilation.fs \
	file.fs \
	load.fs \
	include.fs \
	include-paths.fs \
	executive.fs \
	startup.fs

# test suite
test = 	test/core-system-tests.fs \
	test/stack.fs \
	test/arithmetic.fs \
	test/words.fs \
	test/allot.fs \
	test/ascii.fs \
	test/strings.fs \
	test/evaluate.fs \
	test/formatting.fs \
	test/loops.fs \
	test/conditional-compilation.fs \
	test/continuations.fs \
	test/hooks.fs \
	test/chains.fs \
	test/parser.fs


# ----- Distribution -----

# Distributed files
nobase_dist_pkgdata_DATA = $(forth) $(runtime) $(crosscompiler) $(host) $(regular) $(test)
nobase_dist_doc_DATA = attila.html

# Files to be cleaned
DISTCLEANFILES = primitives.c attila.c target/host/x-host-initial.fs
CLEANFILES = primitives.c attila.c

# Make sure the data directories are readable by everyone
install-data-hook:
	chmod -R a+x $(pkgdatadir)


