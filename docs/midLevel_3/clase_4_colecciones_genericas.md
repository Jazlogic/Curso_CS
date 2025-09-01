# 🚀 Clase 4: Colecciones Genéricas

## 📋 Información de la Clase

- **Módulo**: Mid Level 3 - Manejo de Excepciones y Generics
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 3 (Generics Avanzados)

## 🎯 Objetivos de Aprendizaje

- Implementar colecciones genéricas personalizadas avanzadas
- Crear estructuras de datos genéricas especializadas
- Implementar algoritmos de ordenamiento y búsqueda genéricos
- Usar colecciones genéricas para optimización de memoria

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_manejo_excepciones.md) | Manejo de Excepciones | |
| [Clase 2](clase_2_generics_basicos.md) | Generics Básicos | |
| [Clase 3](clase_3_generics_avanzados.md) | Generics Avanzados | ← Anterior |
| **Clase 4** | **Colecciones Genéricas** | ← Estás aquí |
| [Clase 5](clase_5_interfaces_genericas.md) | Interfaces Genéricas | Siguiente → |
| [Clase 6](clase_6_restricciones_generics.md) | Restricciones de Generics | |
| [Clase 7](clase_7_generics_reflection.md) | Generics y Reflection | |
| [Clase 8](clase_8_generics_performance.md) | Generics y Performance | |
| [Clase 9](clase_9_patrones_generics.md) | Patrones con Generics | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Biblioteca | |

**← [Volver al README del Módulo 3](../midLevel_3/README.md)**

---

## 📚 Contenido Teórico

### 1. Colecciones Genéricas Avanzadas

Las colecciones genéricas personalizadas permiten crear estructuras de datos optimizadas para casos de uso específicos.

```csharp
// ===== COLECCIONES GENÉRICAS - IMPLEMENTACIÓN COMPLETA =====
namespace GenericCollections
{
    // ===== ÁRBOL BINARIO GENÉRICO =====
    namespace GenericBinaryTree
    {
        public class BinaryTreeNode<T> where T : IComparable<T>
        {
            public T Data { get; set; }
            public BinaryTreeNode<T> Left { get; set; }
            public BinaryTreeNode<T> Right { get; set; }
            public int Height { get; set; }
            
            public BinaryTreeNode(T data)
            {
                Data = data;
                Height = 1;
            }
            
            public bool IsLeaf => Left == null && Right == null;
            public bool HasLeftChild => Left != null;
            public bool HasRightChild => Right != null;
        }
        
        public class BinaryTree<T> where T : IComparable<T>
        {
            private BinaryTreeNode<T> _root;
            private int _count;
            
            public int Count => _count;
            public bool IsEmpty => _root == null;
            
            public void Insert(T data)
            {
                _root = InsertRecursive(_root, data);
                _count++;
            }
            
            private BinaryTreeNode<T> InsertRecursive(BinaryTreeNode<T> node, T data)
            {
                if (node == null)
                {
                    return new BinaryTreeNode<T>(data);
                }
                
                int comparison = data.CompareTo(node.Data);
                
                if (comparison < 0)
                {
                    node.Left = InsertRecursive(node.Left, data);
                }
                else if (comparison > 0)
                {
                    node.Right = InsertRecursive(node.Right, data);
                }
                else
                {
                    // Duplicate data, update existing node
                    node.Data = data;
                    _count--; // Don't count as new insertion
                }
                
                return node;
            }
            
            public bool Contains(T data)
            {
                return ContainsRecursive(_root, data);
            }
            
            private bool ContainsRecursive(BinaryTreeNode<T> node, T data)
            {
                if (node == null)
                {
                    return false;
                }
                
                int comparison = data.CompareTo(node.Data);
                
                if (comparison == 0)
                {
                    return true;
                }
                else if (comparison < 0)
                {
                    return ContainsRecursive(node.Left, data);
                }
                else
                {
                    return ContainsRecursive(node.Right, data);
                }
            }
            
            public void Remove(T data)
            {
                _root = RemoveRecursive(_root, data);
                _count--;
            }
            
            private BinaryTreeNode<T> RemoveRecursive(BinaryTreeNode<T> node, T data)
            {
                if (node == null)
                {
                    return null;
                }
                
                int comparison = data.CompareTo(node.Data);
                
                if (comparison < 0)
                {
                    node.Left = RemoveRecursive(node.Left, data);
                }
                else if (comparison > 0)
                {
                    node.Right = RemoveRecursive(node.Right, data);
                }
                else
                {
                    // Node to delete found
                    if (node.IsLeaf)
                    {
                        return null;
                    }
                    else if (!node.HasLeftChild)
                    {
                        return node.Right;
                    }
                    else if (!node.HasRightChild)
                    {
                        return node.Left;
                    }
                    else
                    {
                        // Node has both children
                        var successor = FindMin(node.Right);
                        node.Data = successor.Data;
                        node.Right = RemoveRecursive(node.Right, successor.Data);
                    }
                }
                
                return node;
            }
            
            private BinaryTreeNode<T> FindMin(BinaryTreeNode<T> node)
            {
                while (node.HasLeftChild)
                {
                    node = node.Left;
                }
                return node;
            }
            
            public IEnumerable<T> TraverseInOrder()
            {
                var result = new List<T>();
                TraverseInOrderRecursive(_root, result);
                return result;
            }
            
            private void TraverseInOrderRecursive(BinaryTreeNode<T> node, List<T> result)
            {
                if (node != null)
                {
                    TraverseInOrderRecursive(node.Left, result);
                    result.Add(node.Data);
                    TraverseInOrderRecursive(node.Right, result);
                }
            }
            
            public IEnumerable<T> TraversePreOrder()
            {
                var result = new List<T>();
                TraversePreOrderRecursive(_root, result);
                return result;
            }
            
            private void TraversePreOrderRecursive(BinaryTreeNode<T> node, List<T> result)
            {
                if (node != null)
                {
                    result.Add(node.Data);
                    TraversePreOrderRecursive(node.Left, result);
                    TraversePreOrderRecursive(node.Right, result);
                }
            }
            
            public IEnumerable<T> TraversePostOrder()
            {
                var result = new List<T>();
                TraversePostOrderRecursive(_root, result);
                return result;
            }
            
            private void TraversePostOrderRecursive(BinaryTreeNode<T> node, List<T> result)
            {
                if (node != null)
                {
                    TraversePostOrderRecursive(node.Left, result);
                    TraversePostOrderRecursive(node.Right, result);
                    result.Add(node.Data);
                }
            }
            
            public IEnumerable<T> TraverseLevelOrder()
            {
                var result = new List<T>();
                if (_root == null) return result;
                
                var queue = new Queue<BinaryTreeNode<T>>();
                queue.Enqueue(_root);
                
                while (queue.Count > 0)
                {
                    var node = queue.Dequeue();
                    result.Add(node.Data);
                    
                    if (node.HasLeftChild)
                    {
                        queue.Enqueue(node.Left);
                    }
                    
                    if (node.HasRightChild)
                    {
                        queue.Enqueue(node.Right);
                    }
                }
                
                return result;
            }
        }
    }
    
    // ===== HEAP GENÉRICO =====
    namespace GenericHeap
    {
        public class Heap<T> where T : IComparable<T>
        {
            private readonly List<T> _elements;
            private readonly bool _isMaxHeap;
            
            public Heap(bool isMaxHeap = true)
            {
                _elements = new List<T>();
                _isMaxHeap = isMaxHeap;
            }
            
            public int Count => _elements.Count;
            public bool IsEmpty => _elements.Count == 0;
            
            public void Insert(T element)
            {
                _elements.Add(element);
                HeapifyUp(_elements.Count - 1);
            }
            
            public T ExtractRoot()
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Heap is empty");
                }
                
                var root = _elements[0];
                var lastElement = _elements[_elements.Count - 1];
                _elements[0] = lastElement;
                _elements.RemoveAt(_elements.Count - 1);
                
                if (!IsEmpty)
                {
                    HeapifyDown(0);
                }
                
                return root;
            }
            
            public T Peek()
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Heap is empty");
                }
                
                return _elements[0];
            }
            
            private void HeapifyUp(int index)
            {
                var parentIndex = GetParentIndex(index);
                
                if (parentIndex >= 0 && ShouldSwap(parentIndex, index))
                {
                    Swap(parentIndex, index);
                    HeapifyUp(parentIndex);
                }
            }
            
            private void HeapifyDown(int index)
            {
                var largestIndex = index;
                var leftChildIndex = GetLeftChildIndex(index);
                var rightChildIndex = GetRightChildIndex(index);
                
                if (leftChildIndex < _elements.Count && ShouldSwap(largestIndex, leftChildIndex))
                {
                    largestIndex = leftChildIndex;
                }
                
                if (rightChildIndex < _elements.Count && ShouldSwap(largestIndex, rightChildIndex))
                {
                    largestIndex = rightChildIndex;
                }
                
                if (largestIndex != index)
                {
                    Swap(index, largestIndex);
                    HeapifyDown(largestIndex);
                }
            }
            
            private bool ShouldSwap(int parentIndex, int childIndex)
            {
                var comparison = _elements[parentIndex].CompareTo(_elements[childIndex]);
                return _isMaxHeap ? comparison < 0 : comparison > 0;
            }
            
            private void Swap(int index1, int index2)
            {
                var temp = _elements[index1];
                _elements[index1] = _elements[index2];
                _elements[index2] = temp;
            }
            
            private int GetParentIndex(int index) => (index - 1) / 2;
            private int GetLeftChildIndex(int index) => 2 * index + 1;
            private int GetRightChildIndex(int index) => 2 * index + 2;
            
            public void Clear()
            {
                _elements.Clear();
            }
            
            public T[] ToArray()
            {
                return _elements.ToArray();
            }
        }
        
        public class MinHeap<T> : Heap<T> where T : IComparable<T>
        {
            public MinHeap() : base(false) { }
        }
        
        public class MaxHeap<T> : Heap<T> where T : IComparable<T>
        {
            public MaxHeap() : base(true) { }
        }
    }
    
    // ===== LISTA ENLAZADA DOBLE GENÉRICA =====
    namespace GenericDoublyLinkedList
    {
        public class DoublyLinkedListNode<T>
        {
            public T Data { get; set; }
            public DoublyLinkedListNode<T> Previous { get; set; }
            public DoublyLinkedListNode<T> Next { get; set; }
            
            public DoublyLinkedListNode(T data)
            {
                Data = data;
            }
        }
        
        public class DoublyLinkedList<T>
        {
            private DoublyLinkedListNode<T> _head;
            private DoublyLinkedListNode<T> _tail;
            private int _count;
            
            public int Count => _count;
            public bool IsEmpty => _count == 0;
            
            public T First => _head?.Data;
            public T Last => _tail?.Data;
            
            public void AddFirst(T data)
            {
                var newNode = new DoublyLinkedListNode<T>(data);
                
                if (IsEmpty)
                {
                    _head = _tail = newNode;
                }
                else
                {
                    newNode.Next = _head;
                    _head.Previous = newNode;
                    _head = newNode;
                }
                
                _count++;
            }
            
            public void AddLast(T data)
            {
                var newNode = new DoublyLinkedListNode<T>(data);
                
                if (IsEmpty)
                {
                    _head = _tail = newNode;
                }
                else
                {
                    newNode.Previous = _tail;
                    _tail.Next = newNode;
                    _tail = newNode;
                }
                
                _count++;
            }
            
            public void AddAfter(T existingData, T newData)
            {
                var existingNode = FindNode(existingData);
                if (existingNode == null)
                {
                    throw new ArgumentException("Existing data not found in list");
                }
                
                var newNode = new DoublyLinkedListNode<T>(newData);
                
                newNode.Next = existingNode.Next;
                newNode.Previous = existingNode;
                
                if (existingNode.Next != null)
                {
                    existingNode.Next.Previous = newNode;
                }
                else
                {
                    _tail = newNode;
                }
                
                existingNode.Next = newNode;
                _count++;
            }
            
            public void AddBefore(T existingData, T newData)
            {
                var existingNode = FindNode(existingData);
                if (existingNode == null)
                {
                    throw new ArgumentException("Existing data not found in list");
                }
                
                var newNode = new DoublyLinkedListNode<T>(newData);
                
                newNode.Previous = existingNode.Previous;
                newNode.Next = existingNode;
                
                if (existingNode.Previous != null)
                {
                    existingNode.Previous.Next = newNode;
                }
                else
                {
                    _head = newNode;
                }
                
                existingNode.Previous = newNode;
                _count++;
            }
            
            public bool Remove(T data)
            {
                var node = FindNode(data);
                if (node == null)
                {
                    return false;
                }
                
                if (node.Previous != null)
                {
                    node.Previous.Next = node.Next;
                }
                else
                {
                    _head = node.Next;
                }
                
                if (node.Next != null)
                {
                    node.Next.Previous = node.Previous;
                }
                else
                {
                    _tail = node.Previous;
                }
                
                _count--;
                return true;
            }
            
            public T RemoveFirst()
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("List is empty");
                }
                
                var data = _head.Data;
                
                if (_head == _tail)
                {
                    _head = _tail = null;
                }
                else
                {
                    _head = _head.Next;
                    _head.Previous = null;
                }
                
                _count--;
                return data;
            }
            
            public T RemoveLast()
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("List is empty");
                }
                
                var data = _tail.Data;
                
                if (_head == _tail)
                {
                    _head = _tail = null;
                }
                else
                {
                    _tail = _tail.Previous;
                    _tail.Next = null;
                }
                
                _count--;
                return data;
            }
            
            private DoublyLinkedListNode<T> FindNode(T data)
            {
                var current = _head;
                while (current != null)
                {
                    if (EqualityComparer<T>.Default.Equals(current.Data, data))
                    {
                        return current;
                    }
                    current = current.Next;
                }
                return null;
            }
            
            public bool Contains(T data)
            {
                return FindNode(data) != null;
            }
            
            public void Clear()
            {
                _head = _tail = null;
                _count = 0;
            }
            
            public T[] ToArray()
            {
                var array = new T[_count];
                var current = _head;
                var index = 0;
                
                while (current != null)
                {
                    array[index++] = current.Data;
                    current = current.Next;
                }
                
                return array;
            }
            
            public IEnumerable<T> GetForwardEnumerator()
            {
                var current = _head;
                while (current != null)
                {
                    yield return current.Data;
                    current = current.Next;
                }
            }
            
            public IEnumerable<T> GetReverseEnumerator()
            {
                var current = _tail;
                while (current != null)
                {
                    yield return current.Data;
                    current = current.Previous;
                }
            }
        }
    }
    
    // ===== COLA DE PRIORIDAD GENÉRICA =====
    namespace GenericPriorityQueue
    {
        public class PriorityQueueItem<T, TPriority> where TPriority : IComparable<TPriority>
        {
            public T Data { get; set; }
            public TPriority Priority { get; set; }
            
            public PriorityQueueItem(T data, TPriority priority)
            {
                Data = data;
                Priority = priority;
            }
        }
        
        public class PriorityQueue<T, TPriority> where TPriority : IComparable<TPriority>
        {
            private readonly List<PriorityQueueItem<T, TPriority>> _items;
            private readonly bool _isMaxPriority;
            
            public PriorityQueue(bool isMaxPriority = true)
            {
                _items = new List<PriorityQueueItem<T, TPriority>>();
                _isMaxPriority = isMaxPriority;
            }
            
            public int Count => _items.Count;
            public bool IsEmpty => _items.Count == 0;
            
            public void Enqueue(T item, TPriority priority)
            {
                var queueItem = new PriorityQueueItem<T, TPriority>(item, priority);
                _items.Add(queueItem);
                HeapifyUp(_items.Count - 1);
            }
            
            public T Dequeue()
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Priority queue is empty");
                }
                
                var item = _items[0].Data;
                var lastItem = _items[_items.Count - 1];
                _items[0] = lastItem;
                _items.RemoveAt(_items.Count - 1);
                
                if (!IsEmpty)
                {
                    HeapifyDown(0);
                }
                
                return item;
            }
            
            public T Peek()
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Priority queue is empty");
                }
                
                return _items[0].Data;
            }
            
            public TPeekResult PeekWithPriority()
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Priority queue is empty");
                }
                
                return new TPeekResult(_items[0].Data, _items[0].Priority);
            }
            
            private void HeapifyUp(int index)
            {
                var parentIndex = GetParentIndex(index);
                
                if (parentIndex >= 0 && ShouldSwap(parentIndex, index))
                {
                    Swap(parentIndex, index);
                    HeapifyUp(parentIndex);
                }
            }
            
            private void HeapifyDown(int index)
            {
                var largestIndex = index;
                var leftChildIndex = GetLeftChildIndex(index);
                var rightChildIndex = GetRightChildIndex(index);
                
                if (leftChildIndex < _items.Count && ShouldSwap(largestIndex, leftChildIndex))
                {
                    largestIndex = leftChildIndex;
                }
                
                if (rightChildIndex < _items.Count && ShouldSwap(largestIndex, rightChildIndex))
                {
                    largestIndex = rightChildIndex;
                }
                
                if (largestIndex != index)
                {
                    Swap(index, largestIndex);
                    HeapifyDown(largestIndex);
                }
            }
            
            private bool ShouldSwap(int parentIndex, int childIndex)
            {
                var comparison = _items[parentIndex].Priority.CompareTo(_items[childIndex].Priority);
                return _isMaxPriority ? comparison < 0 : comparison > 0;
            }
            
            private void Swap(int index1, int index2)
            {
                var temp = _items[index1];
                _items[index1] = _items[index2];
                _items[index2] = temp;
            }
            
            private int GetParentIndex(int index) => (index - 1) / 2;
            private int GetLeftChildIndex(int index) => 2 * index + 1;
            private int GetRightChildIndex(int index) => 2 * index + 2;
            
            public void Clear()
            {
                _items.Clear();
            }
            
            public TPeekResult[] ToArray()
            {
                return _items.Select(item => new TPeekResult(item.Data, item.Priority)).ToArray();
            }
        }
        
        public class TPeekResult<T, TPriority>
        {
            public T Data { get; set; }
            public TPriority Priority { get; set; }
            
            public TPeekResult(T data, TPriority priority)
            {
                Data = data;
                Priority = priority;
            }
        }
    }
    
    // ===== ALGORITMOS DE ORDENAMIENTO GENÉRICOS =====
    namespace GenericSorting
    {
        public static class GenericSorter
        {
            public static void QuickSort<T>(T[] array) where T : IComparable<T>
            {
                QuickSort(array, 0, array.Length - 1);
            }
            
            private static void QuickSort<T>(T[] array, int low, int high) where T : IComparable<T>
            {
                if (low < high)
                {
                    var pivotIndex = Partition(array, low, high);
                    QuickSort(array, low, pivotIndex - 1);
                    QuickSort(array, pivotIndex + 1, high);
                }
            }
            
            private static int Partition<T>(T[] array, int low, int high) where T : IComparable<T>
            {
                var pivot = array[high];
                var i = low - 1;
                
                for (int j = low; j < high; j++)
                {
                    if (array[j].CompareTo(pivot) <= 0)
                    {
                        i++;
                        Swap(array, i, j);
                    }
                }
                
                Swap(array, i + 1, high);
                return i + 1;
            }
            
            public static void MergeSort<T>(T[] array) where T : IComparable<T>
            {
                MergeSort(array, 0, array.Length - 1);
            }
            
            private static void MergeSort<T>(T[] array, int low, int high) where T : IComparable<T>
            {
                if (low < high)
                {
                    var mid = (low + high) / 2;
                    MergeSort(array, low, mid);
                    MergeSort(array, mid + 1, high);
                    Merge(array, low, mid, high);
                }
            }
            
            private static void Merge<T>(T[] array, int low, int mid, int high) where T : IComparable<T>
            {
                var leftArray = new T[mid - low + 1];
                var rightArray = new T[high - mid];
                
                Array.Copy(array, low, leftArray, 0, leftArray.Length);
                Array.Copy(array, mid + 1, rightArray, 0, rightArray.Length);
                
                var i = 0;
                var j = 0;
                var k = low;
                
                while (i < leftArray.Length && j < rightArray.Length)
                {
                    if (leftArray[i].CompareTo(rightArray[j]) <= 0)
                    {
                        array[k] = leftArray[i];
                        i++;
                    }
                    else
                    {
                        array[k] = rightArray[j];
                        j++;
                    }
                    k++;
                }
                
                while (i < leftArray.Length)
                {
                    array[k] = leftArray[i];
                    i++;
                    k++;
                }
                
                while (j < rightArray.Length)
                {
                    array[k] = rightArray[j];
                    j++;
                    k++;
                }
            }
            
            public static void HeapSort<T>(T[] array) where T : IComparable<T>
            {
                var heap = new Heap<T>(false); // Min heap for ascending sort
                
                foreach (var item in array)
                {
                    heap.Insert(item);
                }
                
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = heap.ExtractRoot();
                }
            }
            
            private static void Swap<T>(T[] array, int index1, int index2)
            {
                var temp = array[index1];
                array[index1] = array[index2];
                array[index2] = temp;
            }
        }
    }
}

// Uso de Colecciones Genéricas
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Colecciones Genéricas - Clase 4 ===\n");
        
        Console.WriteLine("Los componentes implementados incluyen:");
        Console.WriteLine("1. Árbol binario genérico con recorridos");
        Console.WriteLine("2. Heap genérico (Min/Max)");
        Console.WriteLine("3. Lista enlazada doble genérica");
        Console.WriteLine("4. Cola de prioridad genérica");
        Console.WriteLine("5. Algoritmos de ordenamiento genéricos");
        
        Console.WriteLine("\nEjemplos de uso:");
        
        // Árbol binario
        var tree = new BinaryTree<int>();
        tree.Insert(5);
        tree.Insert(3);
        tree.Insert(7);
        tree.Insert(1);
        tree.Insert(9);
        
        Console.WriteLine($"Árbol contiene 7: {tree.Contains(7)}");
        Console.WriteLine($"Recorrido in-order: {string.Join(", ", tree.TraverseInOrder())}");
        
        // Heap
        var maxHeap = new MaxHeap<int>();
        maxHeap.Insert(10);
        maxHeap.Insert(5);
        maxHeap.Insert(15);
        maxHeap.Insert(3);
        
        Console.WriteLine($"Máximo del heap: {maxHeap.Peek()}");
        Console.WriteLine($"Extraído: {maxHeap.ExtractRoot()}");
        
        // Lista enlazada doble
        var list = new DoublyLinkedList<string>();
        list.AddFirst("Primero");
        list.AddLast("Último");
        list.AddAfter("Primero", "Segundo");
        
        Console.WriteLine($"Primer elemento: {list.First}");
        Console.WriteLine($"Último elemento: {list.Last}");
        
        // Cola de prioridad
        var priorityQueue = new PriorityQueue<string, int>();
        priorityQueue.Enqueue("Baja prioridad", 3);
        priorityQueue.Enqueue("Alta prioridad", 1);
        priorityQueue.Enqueue("Media prioridad", 2);
        
        Console.WriteLine($"Siguiente en cola: {priorityQueue.Peek()}");
        
        // Ordenamiento
        var numbers = new int[] { 64, 34, 25, 12, 22, 11, 90 };
        Console.WriteLine($"Array original: {string.Join(", ", numbers)}");
        
        GenericSorter.QuickSort(numbers);
        Console.WriteLine($"Array ordenado: {string.Join(", ", numbers)}");
        
        Console.WriteLine("\nBeneficios de esta implementación:");
        Console.WriteLine("- Estructuras de datos optimizadas y type-safe");
        Console.WriteLine("- Algoritmos de ordenamiento genéricos");
        Console.WriteLine("- Colecciones especializadas para diferentes casos de uso");
        Console.WriteLine("- Implementaciones eficientes de estructuras clásicas");
        Console.WriteLine("- Flexibilidad para diferentes tipos de datos");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Árbol AVL Genérico
Implementa un árbol AVL genérico que mantenga el balance automáticamente.

### Ejercicio 2: Cola Circular Genérica
Crea una cola circular genérica con capacidad fija y manejo de overflow.

### Ejercicio 3: Grafo Genérico
Implementa un grafo genérico con algoritmos de búsqueda (BFS/DFS).

## 🔍 Puntos Clave

1. **Árbol binario genérico** con múltiples recorridos
2. **Heap genérico** para colas de prioridad
3. **Lista enlazada doble** con navegación bidireccional
4. **Cola de prioridad** con tipos genéricos
5. **Algoritmos de ordenamiento** genéricos

## 📚 Recursos Adicionales

- [Microsoft Docs - Collections](https://docs.microsoft.com/en-us/dotnet/standard/collections/)
- [Data Structures and Algorithms](https://docs.microsoft.com/en-us/dotnet/standard/collections/)

---

**🎯 ¡Has completado la Clase 4! Ahora comprendes las Colecciones Genéricas**

**📚 [Siguiente: Clase 5 - Interfaces Genéricas](clase_5_interfaces_genericas.md)**
