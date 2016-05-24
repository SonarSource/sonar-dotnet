/*
 * SonarQube C# Plugin
 * Copyright (C) 2014-2016 SonarSource SA
 * mailto:contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package org.sonar.plugins.csharp;

import com.google.common.collect.ImmutableList;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.GenericTokenType;
import com.sonar.sslr.api.Token;
import com.sonar.sslr.api.TokenType;
import com.sonar.sslr.impl.Lexer;
import com.sonar.sslr.impl.channel.BlackHoleChannel;
import com.sonar.sslr.impl.channel.BomCharacterChannel;
import com.sonar.sslr.impl.channel.IdentifierAndKeywordChannel;
import com.sonar.sslr.impl.channel.PunctuatorChannel;
import com.sonar.sslr.impl.channel.UnknownCharacterChannel;
import java.io.File;
import java.util.List;
import net.sourceforge.pmd.cpd.SourceCode;
import net.sourceforge.pmd.cpd.TokenEntry;
import net.sourceforge.pmd.cpd.Tokenizer;
import net.sourceforge.pmd.cpd.Tokens;
import org.sonar.api.batch.AbstractCpdMapping;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.resources.Language;

import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.ANY_CHAR;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.DIGIT;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.HEXA_DIGIT;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.anyButNot;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.commentRegexp;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.g;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.o2n;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.one2n;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.opt;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.or;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.regexp;

public class CSharpCPDMapping extends AbstractCpdMapping {

  private static final String INT_SUFFIX = "(((U|u)(L|l)?)|((L|l)(u|U)?))";
  private static final String REAL_SUFFIX = "(F|f|D|d|M|m)";
  private static final String EXP = g("[Ee]" + opt("[+-]") + one2n(DIGIT));

  private static final String LETTER_CHAR = g("\\p{Lu}|\\p{Ll}|\\p{Lt}|\\p{Lm}|\\p{Lo}|\\p{Nl}");
  private static final String COMBINING_CHAR = g("\\p{Mn}|\\p{Mc}");
  private static final String DECIMAL_DIGIT_CHAR = g("\\p{Nd}");
  private static final String CONNECTING_CHAR = g("\\p{Pc}");
  private static final String FORMATTING_CHAR = g("\\p{Cf}");

  private enum CSharpPunctuator implements TokenType {
    SEMICOLON(";"), EQUAL("="), STAR("*"), LCURLYBRACE("{"), LPARENTHESIS("("), LBRACKET("["), RBRACKET("]"), RPARENTHESIS(")"), RCURLYBRACE(
      "}"), COLON(":"), COMMA(","), DOT("."), EXCLAMATION("!"), SUPERIOR(">"), INFERIOR("<"), PLUS("+"), MINUS("-"), SLASH("/"), MODULO("%"), AND(
      "&"), XOR("^"), OR("|"), QUESTION("?"), TILDE("~"), DOUBLE_COLON("::"), DOUBLE_QUESTION("??"), EQ_OP("=="), NE_OP("!="), LEFT_ASSIGN(
      "<<="), ADD_ASSIGN("+="), SUB_ASSIGN("-="), MUL_ASSIGN("*="), DIV_ASSIGN("/="), MOD_ASSIGN("%="), AND_ASSIGN("&="), XOR_ASSIGN("^="), OR_ASSIGN(
      "|="), LEFT_OP("<<"), INC_OP("++"), DEC_OP("--"), PTR_OP("->"), AND_OP("&&"), OR_OP("||"), LE_OP("<="), GE_OP(">="), LAMBDA("=>");

    private final String value;

    CSharpPunctuator(String word) {
      this.value = word;
    }

    @Override
    public String getName() {
      return name();
    }

    @Override
    public String getValue() {
      return value;
    }

    @Override
    public boolean hasToBeSkippedFromAst(AstNode node) {
      return false;
    }

  }

  private final CSharp csharp;
  private final Lexer lexer;

  public CSharpCPDMapping(CSharp csharp, FileSystem fs) {
    this.csharp = csharp;

    this.lexer = Lexer.builder()
      .withCharset(fs.encoding())

      .withFailIfNoChannelToConsumeOneCharacter(true)

      // Comments
      .withChannel(commentRegexp("//", o2n("[^\\n\\r]")))
      .withChannel(commentRegexp("/\\*", ANY_CHAR + "*?", "\\*/"))
      // Literals : Strings
      .withChannel(regexp(GenericTokenType.LITERAL, "\"", o2n(or("\\\\.", anyButNot("\"", "\\n", "\\r"))), "\""))
      .withChannel(regexp(GenericTokenType.LITERAL, "@\"", o2n(or("\"\"", anyButNot("\""))), "\""))
      // Literals : Character
      .withChannel(regexp(GenericTokenType.IDENTIFIER, "'", one2n(or("\\\\.", anyButNot("'", "\\n", "\\r"))), "'"))
      // Literals : Reals
      .withChannel(regexp(GenericTokenType.IDENTIFIER, o2n(DIGIT), "\\.", one2n(DIGIT), opt(EXP), opt(REAL_SUFFIX)))
      .withChannel(regexp(GenericTokenType.IDENTIFIER, one2n(DIGIT), EXP, opt(REAL_SUFFIX)))
      .withChannel(regexp(GenericTokenType.IDENTIFIER, one2n(DIGIT), REAL_SUFFIX))
      // Literals : Integers
      .withChannel(regexp(GenericTokenType.IDENTIFIER, "0[xX]", one2n(HEXA_DIGIT), opt(INT_SUFFIX)))
      .withChannel(regexp(GenericTokenType.IDENTIFIER, one2n(DIGIT), opt(INT_SUFFIX)))
      // Identifiers, keywords, punctuators and operators
      .withChannel(new IdentifierAndKeywordChannel(
        g(
          opt("@"),
          or(
            LETTER_CHAR,
            "_"),
          o2n(
          or(
            LETTER_CHAR,
            DECIMAL_DIGIT_CHAR,
            CONNECTING_CHAR,
            COMBINING_CHAR,
            FORMATTING_CHAR))),
        true))
      .withChannel(new PunctuatorChannel(CSharpPunctuator.values()))
      // Preprocessor directives
      .withChannel(regexp(GenericTokenType.IDENTIFIER, "#[^\\r\\n]*"))
      // Others
      .withChannel(new BlackHoleChannel("\\s++"))
      .withChannel(new BomCharacterChannel())
      .withChannel(new UnknownCharacterChannel())

      .build();
  }

  @Override
  public Language getLanguage() {
    return csharp;
  }

  @Override
  public Tokenizer getTokenizer() {
    return new Tokenizer() {

      @Override
      public void tokenize(SourceCode source, Tokens cpdTokens) {
        String fileName = source.getFileName();
        List<Token> tokens = lexer.lex(new File(fileName));
        tokens = removeUsingDirectives(tokens);
        for (Token token : tokens) {
          if (GenericTokenType.EOF.equals(token.getType())) {
            break;
          }

          TokenEntry cpdToken = new TokenEntry(getTokenImage(token), fileName, token.getLine());
          cpdTokens.add(cpdToken);
        }
        cpdTokens.add(TokenEntry.getEOF());
      }

      private String getTokenImage(Token token) {
        if (GenericTokenType.LITERAL.equals(token.getType())) {
          return GenericTokenType.LITERAL.getValue();
        }
        return token.getValue();
      }

      private List<Token> removeUsingDirectives(List<Token> tokens) {
        ImmutableList.Builder<Token> builder = ImmutableList.builder();

        for (int i = 0; i < tokens.size() - 1; i++) {
          Token token = tokens.get(i);
          if ("using".equals(token.getOriginalValue()) && !"(".equals(tokens.get(i + 1).getOriginalValue())) {
            while (i < tokens.size() - 1 && !";".equals(tokens.get(i).getOriginalValue())) {
              i++;
            }
          } else {
            builder.add(token);
          }
        }

        builder.add(tokens.get(tokens.size() - 1));

        return builder.build();
      }

    };
  }

}
