﻿using System.Runtime.CompilerServices;

namespace Stashbox.Utils.Data
{
    internal sealed class HashTree<TKey, TValue>
    {
        private class Node
        {
            public readonly int storedHash;
            public readonly TKey storedKey;
            public TValue storedValue;
            public Node left;
            public Node right;
            public int height;
            public ExpandableArray<TKey, TValue> collisions;

            public Node(TKey key, TValue value, int hash)
            {
                this.storedValue = value;
                this.storedKey = key;
                this.storedHash = hash;
                this.height = 1;
            }
        }

        private Node root;

        public HashTree() { }

        public void Add(TKey key, TValue value, bool byRef = true)
        {
            this.root = Add(this.root, key, byRef ? RuntimeHelpers.GetHashCode(key) : key.GetHashCode(), value, byRef);
        }

        [MethodImpl(Constants.Inline)]
        public TValue GetOrDefaultByValue(TKey key)
        {
            if (this.root == null)
                return default;

            var node = root;
            var hash = key.GetHashCode();
            while (node != null && node.storedHash != hash)
                node = hash < node.storedHash ? node.left : node.right;
            return node != null && Equals(key, node.storedKey)
                ? node.storedValue
                : node?.collisions == null
                    ? default
                    : node.collisions.GetOrDefaultByValue(key);
        }

        private static int CalculateHeight(Node node)
        {
            if (node.left != null && node.right != null)
                return 1 + (node.left.height > node.right.height ? node.left.height : node.right.height);

            if (node.left == null && node.right == null)
                return 1;

            return 1 + (node.left?.height ?? node.right.height);
        }

        private static int GetBalance(Node node)
        {
            if (node.left != null && node.right != null)
                return node.left.height - node.right.height;

            if (node.left == null && node.right == null)
                return 0;

            return node.left?.height ?? node.right.height * -1;
        }

        private static Node RotateLeft(Node node)
        {
            var current = node.right;
            var left = current.left;

            current.left = node;
            node.right = left;

            current.height = CalculateHeight(current);
            node.height = CalculateHeight(node);

            return current;
        }

        private static Node RotateRight(Node node)
        {
            var current = node.left;
            var right = current.right;
            current.right = node;
            node.left = right;
            current.height = CalculateHeight(current);
            node.height = CalculateHeight(node);

            return current;
        }

        private static Node Add(Node node, TKey key, int hash, TValue value, bool byRef)
        {
            if (node == null)
                return new Node(key, value, hash);

            if (node.storedHash == hash)
            {
                CheckCollisions(node, key, value, byRef);
                return node;
            }

            if (node.storedHash > hash)
                node.left = Add(node.left, key, hash, value, byRef);
            else
                node.right = Add(node.right, key, hash, value, byRef);

            node.height = CalculateHeight(node);
            var balance = GetBalance(node);

            if (balance >= 2)
            {
                if (GetBalance(node.left) == -1)
                {
                    node.left = RotateLeft(node.left);
                    node = RotateRight(node);
                }
                else
                    node = RotateRight(node);
            }

            if (balance <= -2)
            {
                if (GetBalance(node.right) == 1)
                {
                    node.right = RotateRight(node.right);
                    node = RotateLeft(node);
                }
                else
                    node = RotateLeft(node);
            }

            return node;
        }

        private static void CheckCollisions(Node node, TKey key, TValue value, bool byRef)
        {
            if (byRef && ReferenceEquals(key, node.storedKey) || !byRef && Equals(key, node.storedKey))
                node.storedValue = value;
            else
            {
                node.collisions ??= new ExpandableArray<TKey, TValue>();
                node.collisions.Add(new KeyValue<TKey, TValue>(key, value));
            }
        }
    }
}
