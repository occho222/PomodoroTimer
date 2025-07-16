# �񓚌���ݒ�

���̃v���W�F�N�g�ł́A�ȉ��̌���ݒ���̗p���܂��F

## ��{���j
- **�S�Ẳ񓚂͓��{��ōs��**
- **�R�����g�����{��ŋL�ڂ���**
- **�ϐ����E�N���X���͉p��Ƃ���**�i���ۓI�ȕێ琫���l���j

## ��̓I�ȃK�C�h���C��

### �R�[�h�R�����g
```csharp
// ? �ǂ���
/// <summary>
/// �|���h�[���^�C�}�[�̃��C���r���[���f��
/// </summary>
public partial class MainViewModel : ObservableObject
{
    // �^�C�}�[�̏�ԊǗ��p
    private DispatcherTimer _timer;
    
    // ���݂̎c�莞��
    [ObservableProperty]
    private string timeRemaining = "25:00";
}

// ? ������
/// <summary>
/// Main view model for pomodoro timer
/// </summary>
public partial class MainViewModel : ObservableObject
{
    // Timer state management
    private DispatcherTimer _timer;
}
```

### XAML���̕\���e�L�X�g
```xaml
<!-- ? �ǂ��� -->
<TextBlock Text="�����̃^�X�N" FontSize="20" FontWeight="Bold"/>
<Button Content="�J�n" Command="{Binding StartCommand}"/>

<!-- ? ������ -->
<TextBlock Text="Today's Tasks" FontSize="20" FontWeight="Bold"/>
<Button Content="Start" Command="{Binding StartCommand}"/>
```

### �G���[���b�Z�[�W�E�ʒm
```csharp
// ? �ǂ���
MessageBox.Show("�^�X�N������͂��Ă��������B", "�G���[", MessageBoxButton.OK, MessageBoxImage.Warning);

// ? ������
MessageBox.Show("Please enter task name.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
```

### �ϐ����E���\�b�h��
```csharp
// ? �ǂ���i�p��j
public partial class PomodoroTask : ObservableObject
{
    [ObservableProperty]
    private string title = string.Empty;
    
    [ObservableProperty]
    private int estimatedPomodoros = 1;
    
    public void StartTask()
    {
        // �^�X�N���J�n���鏈��
    }
}

// ? ������i���{��j
public partial class PomodoroTask : ObservableObject
{
    [ObservableProperty]
    private string �^�C�g�� = string.Empty;
    
    public void �^�X�N�J�n()
    {
        // ����
    }
}
```

## �R�~�b�g���[��

### ��{����
- **���m��**: �ύX���e����ڂŕ�����K�؂ȓ��{��̃R�����g�ŃR�~�b�g���Ă�������
- **�^�C�~���O**: �R�~�b�g�́u�R�~�b�g�̖��߁v���������Ƃ��̂ݎ��s���Ă�������
- **���S��**: �G���[������邽�ߓ��ꕶ���̎g�p�͍T���Ă�������

### �d�v�Ȓ��ӎ���
- **�֎~����**: �R�����i`:`�j�A�Z�~�R�����i`;`�j�A���ʂȂǂ�PowerShell�ŃG���[�̌����ƂȂ�܂�
- **��������**: ���{��A�p�����A�n�C�t���i`-`�j�A�A���_�[�X�R�A�i`_`�j���g�p���Ă�������

### �R�~�b�g���b�Z�[�W�̗�

#### �ǂ���
```
git commit -m "�^�C�}�[�@�\�̒ǉ�"
git commit -m "UI���C�A�E�g�̉��P"
git commit -m "�ݒ��ʂ̎���"
git commit -m "�o�O�C��_�^�X�N�폜�@�\"
git commit -m "�h�L�������g�X�V-�J���w�j"
```

#### ������ׂ���
```
git commit -m "�@�\�ǉ�: �^�C�}�[����"  # �R�����̓G���[�̌���
git commit -m "�C��(�ً})"            # ���ʂ̓G���[�̌���
git commit -m "Update; fix bugs"      # �Z�~�R�����̓G���[�̌���
```

### �R�~�b�g���b�Z�[�W�̃K�C�h���C��
1. **�����\���������g�p**: �ǉ��A�C���A�폜�A�X�V�A����
2. **��̓I�ȕύX���e���L��**: ����ύX�������𖾊m��
3. **50�����ȓ�**: �Ȍ��ŕ�����₷��
4. **���ꕶ���������**: PowerShell�G���[��h��

### �^�C�~���O���[��
- �R�~�b�g�͊J���҂��u�R�~�b�g�v��u�ۑ��v�𖾎��I�Ɏw�������Ƃ��̂ݎ��s
- �����I�ȃR�~�b�g�͍s��Ȃ�
- �ύX������A����m�F���s���Ă���R�~�b�g

## AI�񓚎��̕��j

### �����E���
- �Z�p�I�Ȑ����͓��{��ōs��
- �R�[�h�̉�������{��ŋL��
- �x�X�g�v���N�e�B�X�̐��������{��

### �R�[�h���
- �R�����g�͓��{��
- �ϐ����E���\�b�h���͉p��
- UI�\���e�L�X�g�͓��{��

### �G���[�Ή�
- �G���[���b�Z�[�W�̐����͓��{��
- �������@�̒�Ă����{��
- ���[�U�[�������b�Z�[�W�͓��{��

���̐ݒ�ɂ��A���{��ł̊J���E�ێ���s���Ȃ���A���ۓI�ȊJ���`�[���ł��������₷���R�[�h�x�[�X���ێ����܂��B