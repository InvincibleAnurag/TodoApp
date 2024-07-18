import React, { useState, useEffect } from 'react';
import axios from 'axios';

const API_URL = 'http://localhost:5099/todos';

const fetchTodos = async () => {
  const response = await axios.get(API_URL);
  return response.data;
};

const createTodo = async (todo) => {
  const response = await axios.post(API_URL, todo);
  return response.data;
};

const updateTodo = async (id, todo) => {
  const response = await axios.put(`${API_URL}/${id}`, todo);
  return response.data;
};

const deleteTodo = async (id) => {
  await axios.delete(`${API_URL}/${id}`);
};

function App() {
  const [todos, setTodos] = useState([]);
  const [newTodo, setNewTodo] = useState('');
  const [editingTodo, setEditingTodo] = useState(null);
  const [editingTitle, setEditingTitle] = useState('');

  useEffect(() => {
    fetchTodos().then(data => setTodos(data));
  }, []);

  const handleCreateTodo = async () => {
    if (newTodo.trim()) {
      await createTodo({ title: newTodo, isCompleted: false });
      setNewTodo('');
      fetchTodos().then(data => setTodos(data));
    }
  };

  const handleUpdateTodo = async (id) => {
    await updateTodo(id, { ...editingTodo, title: editingTitle });
    setEditingTodo(null);
    setEditingTitle('');
    fetchTodos().then(data => setTodos(data));
  };

  const handleDeleteTodo = async (id) => {
    await deleteTodo(id);
    fetchTodos().then(data => setTodos(data));
  };

  return (
    <div className="App">
      <h1>Todo List</h1>
      <div>
        <input
          type="text"
          placeholder="New Todo"
          value={newTodo}
          onChange={(e) => setNewTodo(e.target.value)}
        />
        <button onClick={handleCreateTodo}>Add Todo</button>
      </div>
      <ul>
        {todos.map((todo) => (
          <li key={todo.id}>
            {editingTodo?.id === todo.id ? (
              <div>
                <input
                  type="text"
                  value={editingTitle}
                  onChange={(e) => setEditingTitle(e.target.value)}
                />
                <button onClick={() => handleUpdateTodo(todo.id)}>
                  Save
                </button>
                <button onClick={() => setEditingTodo(null)}>Cancel</button>
              </div>
            ) : (
              <div>
                <span
                  style={{
                    textDecoration: todo.isCompleted ? 'line-through' : 'none'
                  }}
                >
                  {todo.title}
                </span>
                <button onClick={() => {
                  setEditingTodo(todo);
                  setEditingTitle(todo.title);
                }}>
                  Edit
                </button>
                <button onClick={() => handleDeleteTodo(todo.id)}>
                  Delete
                </button>
              </div>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
}

export default App;
