Given a string and a number _x_, `StringEx.FindSpaces` returns the first _x_ number of space characters. Why not Split? Because

  * We are dealing with different types of messages loosely related by syntax but not by number of spaces.
  * There's no fun way to get Split to return a variable number of split strings (there's a dead-easy way to do it for a constant number), a variable that depends on when the message or reason begins.

For example, we'd like to parse ":nick!ip part #channel :reason" and ":nick!ip quit :reason" the same way. For Split, we'd have to do conditionals to figure out what the magic number is. With spaces, all we have to is _perform a conditional on what type of command we're parsing_.

After that, we can use the same "interface" to parse the string--the `string[] spaces` array and `StringEx.Tween(line, a, b)` (the equivalent of `line[a..b-1]` in Ruby).

Comments have the general form of the IRC server message contained. Figuring out what each `spaces[i]` represents is as simple as consulting the comment, I hope.

## See Also ##
  * [PrivmsgFilter](http://theminds.googlecode.com/svn/trunk/Theminds/Filters/PrivmsgFilter.cs)
  * [JoinPartQuitFilter.cs](http://theminds.googlecode.com/svn/trunk/Theminds/Filters/JoinPartQuitFilter.cs)