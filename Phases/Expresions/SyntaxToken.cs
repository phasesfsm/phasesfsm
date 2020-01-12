using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phases.Expresions
{
    class SyntaxToken : Token
    {
        public enum Qualifiers
        {
            Correct,
            Wrong,
            Unexpected,
            NonClosed,
            InvalidUseOf,
        }

        SyntaxToken father, leftChild, rightChild;
        Qualifiers qualifier;

        public SyntaxToken(Token token, Qualifiers qualifier)
            : base(token.Type, token.Text, token.SourceIndex)
        {
            this.qualifier = qualifier;
        }

        public Qualifiers Qualifier => qualifier;

        public override string ToString()
        {
            return qualifier.ToString() + " " + base.ToString();
        }

        public SyntaxToken Father
        {
            get
            {
                return father;
            }
        }

        public SyntaxToken LeftChild
        {
            get
            {
                return leftChild;
            }
            set
            {
                if(value != leftChild)
                {
                    if(leftChild != null)
                    {
                        leftChild.father = null;
                    }
                    leftChild = value;
                    if(value != null) leftChild.father = this;
                }
            }
        }

        public SyntaxToken RightChild
        {
            get
            {
                return rightChild;
            }
            set
            {
                if(value != rightChild)
                {
                    if(rightChild != null)
                    {
                        rightChild.father = null;
                    }
                    rightChild = value;
                    if (value != null) rightChild.father = this;
                }
            }
        }
    }
}
