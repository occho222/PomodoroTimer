# WPF MVVM�J���w�j

���̃h�L�������g�́APomodoroTimer�v���W�F�N�g�ɂ�����WPF MVVM�J���̃x�X�g�v���N�e�B�X���܂Ƃ߂����̂ł��B

## 1. MVVM�p�^�[���̓O��

### ��{����
- **Model-View-ViewModel�iMVVM�j** �ɂ���āA�r���[�iXAML�j�ƃ��W�b�N�iViewModel/Model�j�𖾊m�ɕ�������
- ViewModel�ɂ�UI�Ɋւ��郍�W�b�N�݂̂��L�q���A�r�W�l�X���W�b�N��f�[�^������Model���ɕ�������
- �e�X�g�e�Ր��ƊJ�������̌�����d������

### �����K�C�h���C��
- `ObservableObject` ���p������ViewModel�N���X���쐬
- CommunityToolkit.Mvvm �� `[ObservableProperty]` ���������p
- �r�W�l�X���W�b�N�͐�p��Service�N���X��Model�N���X�ɕ���

## 2. Data Binding �� INotifyPropertyChanged �̊��p

### �o�C���f�B���O�헪
- XAML�ɂ�� **�錾�IUI��`�iData Binding�j** ���ő�����p
- ViewModel �� Model �� `INotifyPropertyChanged` ���������āAUI�X�V�̎������ƃ��A���^�C���ȉ�����S��
- OneTime / OneWay / TwoWay �Ƃ������o�C���h���[�h��K�؂Ɏg������

### �p�t�H�[�}���X�z��
- �ߏ�ȃo�C���h�ɂ��p�t�H�[�}���X�ቺ�ɒ���
- `UpdateSourceTrigger` ��K�؂ɐݒ�
- �s�v�ȑo�����o�C���h�͔�����

### ������
```csharp
[ObservableProperty]
private string timeRemaining = "25:00";

[ObservableProperty]
private bool isRunning = false;
```

## 3. �R�}���h�p�^�[���̊��p

### �R�}���h�������j
- UI��̑���i�{�^�������Ȃǁj�� `ICommand` ���g�����f�[�^�o�C���h�Ŏ���
- �R�[�h�E�r�n�C���h���ɗ͔r��
- CommunityToolkit.Mvvm �� `[RelayCommand]` ���������p

### ������
```csharp
[RelayCommand]
private void StartPause()
{
    // �^�C�}�[�̊J�n/�ꎞ��~���W�b�N
}

[RelayCommand]
private void AddTask()
{
    // �^�X�N�ǉ����W�b�N
}
```

## 4. XAML�X�^�C���ƃe���v���[�g�݌v

### �X�^�C���Ǘ�
- `App.xaml` �ŃA�v���P�[�V�����S�̂̃X�^�C�����`
- Styles / Templates / DataTemplates ����g���āAUI�̍ė��p����e�[�}�Ή�������
- �J���[�p���b�g�����\�[�X�f�B�N�V���i���ňꌳ�Ǘ�

### �J�X�^���R���g���[���݌v
- �J�X�^��Control�쐬���̓e���v���[�g�_����ɗ͊ɂ₩�ɂ���
- �e���v���[�g����`���ɗ�O�𓊂��Ȃ��݌v�ɂ���
- �ė��p�\�ȃX�^�C�����쐬

### ������
```xaml
<Style x:Key="RoundButton" TargetType="Button">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border Background="{TemplateBinding Background}" 
                        CornerRadius="20">
                    <ContentPresenter HorizontalAlignment="Center" 
                                      VerticalAlignment="Center"/>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

## 5. ViewModel�ւ̃R�[�h�E�r�n�C���h�֎~�̌���

### ��{���j
- UI���W�b�N���K�v�ȏꍇ�͂ł��邾��XAML�ƃo�C���f�B���O�^�R�}���h�ŕ\��
- �R�[�h�E�r�n�C���h�͍Ō�̎�i�Ƃ��Ďg�p
- ��O�I�ȃP�[�X�i�h���b�O&�h���b�v�A�����UI����j�̂݃R�[�h�E�r�n�C���h������

### ��������O
- �z�b�g�L�[�̓o�^
- ���G�ȃh���b�O&�h���b�v����
- �E�B���h�E���C�t�T�C�N���Ǘ�

## 6. DI�i�ˑ��������j�ƃA�[�L�e�N�`��

### DI�헪
- Microsoft.Extensions.DependencyInjection �����p�i�����I�ɓ����������j
- ���W���[�����E�e�X�g���E�a�����݌v�𑣐i
- �T�[�r�X�N���X�̈ˑ�������������

### �A�[�L�e�N�`���\��
```
������ Models/          # �f�[�^���f��
������ ViewModels/      # �r���[���f��
������ Views/           # �r���[�iXAML�j
������ Services/        # �r�W�l�X���W�b�N
������ Resources/       # ���\�[�X�i�X�^�C���A�|�󓙁j
```

## 7. ViewModel�̐Ӗ�������SOLID����

### �P��ӔC����
- ViewModel �͂����܂� UI ���W�b�N�ɏW��
- �r�W�l�X���W�b�N�� Model �Ɉڂ����ƂŒP��ӔC�̌����𖾊m��
- �e�N���X�͈�̐Ӗ��݂̂�����

### �ˑ��֌W�t�]����
- �C���^�[�t�F�[�X��ʂ����a�����݌v
- �e�X�g�e�Ր��̊m��

## 8. �A�v���\���ƃv���W�F�N�g�\��

### �i�K�I���W
1. **�����i�K**: �P��v���W�F�N�g�ŊJ�n
2. **�����i�K**: Control/View/ViewModel/Model�P�ʂŕ���
3. **���n�i�K**: ���@�\�E�ė��p���̍����\���ɐi��

### �t�H���_�\��
- �@�\�ʂ܂��͐Ӗ��ʂɃt�H���_�𕪊�
- ���O��Ԃƃt�H���_�\������v������

## 9. ���o�c���[�̍œK���ƃp�t�H�[�}���X

### �p�t�H�[�}���X�l������
- StackPanel��Grid�̑��d�l�X�g�������
- ���o�c���[���ł��邾���󂭕��R�ɕۂ�
- �K�v�ɉ����� UIVirtualization �����p
- �񓯊������ŕ`�敉�ׂ��R���g���[��

### �œK����@
- `VirtualizingStackPanel` �̎g�p
- `ItemsControl` �ł̉��z���L����
- �d�������̔񓯊���

## 10. �A�N�Z�V�r���e�B�E���ۉ����ӎ�

### �A�N�Z�V�r���e�B
- WPF �� UI Automation �����p
- �L�[�{�[�h����ւ̑Ή�
- �K�؂ȃt�H�[�J�X����̎���
- �X�N���[�����[�_�[�Ή�

### ���ۉ��ii18n�j
- Resources �� .resx �𗘗p����������Ή�
- ������̃n�[�h�R�[�f�B���O�������
- �J���`���[�ɉ��������t�E���l�t�H�[�}�b�g�Ή�

### ������
```csharp
// �z�b�g�L�[�̓o�^�i�A�N�Z�V�r���e�B����j
var startPauseCommand = new RoutedCommand();
startPauseCommand.InputGestures.Add(new KeyGesture(Key.Space, ModifierKeys.Control));
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

## ���݂̃v���W�F�N�g��

### �K�p�ς݃x�X�g�v���N�e�B�X
? CommunityToolkit.Mvvm �̊��p  
? ObservableObject�p����ObservableProperty�����̎g�p  
? RelayCommand�����ɂ��R�}���h����  
? Data Binding�̊��p  
? XAML�X�^�C���̒�`  
? MVVM�p�^�[���̊�{����  

### ���P��������
?? �r�W�l�X���W�b�N��Service�w�ւ̕���  
?? DI�i�ˑ��������j�̓���  
?? �ݒ�Ǘ��@�\�̎���  
?? ������Ή��̏���  
?? �P�̃e�X�g�̓���  

## �J�����̒��ӎ���

1. **�V�@�\�J����**: �K��MVVM�p�^�[���ɏ]��
2. **�R�[�h�E�r�n�C���h�ǉ���**: �K�v�����\����������
3. **�p�t�H�[�}���X**: ����I�Ƀ������g�p�ʂƕ`��p�t�H�[�}���X���m�F
4. **�e�X�g**: ViewModel�̒P�̃e�X�g���쐬
5. **�h�L�������g**: ���G�ȃ��W�b�N�͕K���R�����g���L��

���̎w�j�ɏ]�����ƂŁA�ێ琫�E�e�X�g�e�Ր��E�g�����ɗD�ꂽWPF�A�v���P�[�V�������J���ł��܂��B