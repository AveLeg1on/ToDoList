using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using static ToDoList.MVVM.Commands.RelayCommands;

namespace ToDoList.MVVM.ViewModel
{
    class TodoViewModel : INotifyPropertyChanged
    {
        private RelayCommand _saveNoteCommand, _uploadNoteCommand, _closeProgCommand,
            _addNote, _clearNoteCommand, _toCompletedNote, _toDeleteNote;

        private string _note, _header, _tasks;

        private int _counterNotes = 1, _noteNumber = 1;

        public string Note
        {
            get => _note;
            set
            {
                _note = value;
                OnPropertyChanged(nameof(Note));
            }
        }

        public string Header
        {
            get => _header;
            set
            {
                _header = value;
                OnPropertyChanged(nameof(Header));
            }
        }

        public string Tasks
        {
            get => _tasks;
            set
            {
                _tasks = value;
                OnPropertyChanged(nameof(Tasks));
            }
        }

        public int CounterNotes
        {
            get => _counterNotes;
            set
            {
                _counterNotes = value;
                OnPropertyChanged(nameof(CounterNotes));
            }
        }

        public int NoteNumber
        {
            get => _noteNumber;
            set
            {
                _noteNumber = value;
                OnPropertyChanged(nameof(NoteNumber));
            }
        }

        public RelayCommand CloseProgram => _closeProgCommand ?? new RelayCommand(obj =>
        {
            Application.Current.Shutdown();
        });

        public RelayCommand SaveNote => _saveNoteCommand ?? new RelayCommand(obj =>
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Все файлы (*.*)|*.*|Текстовые файлы (*.txt)|*.txt";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.DefaultExt = ".txt";
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    if (Header != "" && Header != null)
                    {
                        StreamWriter writer = new StreamWriter(saveFileDialog.FileName);
                        int tempCounter = 1;
                        for (; tempCounter != CounterNotes;)
                        {
                            writer.WriteLine($"| {tempCounter} | {Header} \n{Note} \n| {tempCounter} |\n");
                            tempCounter++;

                        }
                        writer.Dispose();
                        MessageBox.Show("Список задач был успешно сохранен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Заголовок заметки не может быть пустым.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show($"Здесь - {saveFileDialog.FileName} нельзя сохранить заметку.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                catch (Exception)
                {
                    MessageBox.Show("Возникла неизвестна ошибка.\nПопробуйте перезапустить приложение.", "ОШИБКА", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        });

        public RelayCommand UploadNote => _uploadNoteCommand ?? new RelayCommand(obj =>
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Все файлы (*.*)|*.*|Текстовые файлы (*.txt)|*.txt";
            openFileDialog.FilterIndex = 2;
            openFileDialog.DefaultExt = ".txt";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string extFile = openFileDialog.FileName.Remove(0, openFileDialog.FileName.LastIndexOf('.'));
                    if (extFile == ".txt")
                    {
                        StreamReader reader = new StreamReader(openFileDialog.FileName);
                        string line;
                        int maxNumber = 0;

                        while ((line = reader.ReadLine()) != null)
                        {
                            int startIndex = line.IndexOf("|");
                            int endIndex = line.LastIndexOf("|");
                            Tasks += $"{line}\n";

                            if (startIndex != -1 && endIndex != -1)
                            {
                                string numberStr = line.Substring(startIndex + 1, endIndex - startIndex - 1);
                                if (int.TryParse(numberStr, out int number))
                                {
                                    if (number > maxNumber)
                                    {
                                        maxNumber = number;
                                    }
                                }
                            }

                        }
                        reader.Close();
                        CounterNotes = maxNumber + 1;
                        MessageBox.Show("Список задач был успешно загружен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Выбран не тот файл.\nВыберите файл с расширением .txt", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Возникла неизвестна ошибка.\nПопробуйте перезапустить приложение.", "ОШИБКА", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        });

        public RelayCommand AddNote => _addNote ?? new RelayCommand(obj =>
        {
            if (Header != "" && Header != null)
            {
                Tasks += $" | {CounterNotes} | {Header}\n{Note} | {CounterNotes} | \n\n";
                CounterNotes++;
            }
            else
            {
                MessageBox.Show("Заголовок заметки не может быть пустым.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
        });

        public RelayCommand ClearNote => _clearNoteCommand ?? new RelayCommand(obj =>
        {
            MessageBoxResult userResult = MessageBox.Show("Вы точно хотите удалить все заметки, это также очистит список задач.", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (userResult == MessageBoxResult.Yes)
            {
                Header = "";
                Note = "";
                Tasks = "";
                CounterNotes = 1;
            }
            else
            {
                return;
            }

        });

        public RelayCommand ToCompleted => _toCompletedNote ?? new RelayCommand(obj =>
        {
            if (NoteNumber > 0 && Tasks != null)
            {
                string pattern = $@"\| {NoteNumber} \|([\s\S]*?)\| {NoteNumber} \|";
                Tasks = Regex.Replace(Tasks, pattern, $@"| ✓{NoteNumber} |$1| ✓{NoteNumber} |");

            }
        });

        public RelayCommand ToDelete => _toDeleteNote ?? new RelayCommand(obj =>
        {

            if (NoteNumber > 0 && Tasks != null)
            {
                string pattern = $@"\| ✓{NoteNumber} \|[\s\S]*?\| ✓{NoteNumber} \|";
                Tasks = Regex.Replace(Tasks, pattern, "");
            }
            else
            {
                MessageBox.Show("Для того чтобы, удалить заметку отметьте её как выполненную.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        });

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}